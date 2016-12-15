using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Base.Xml;
using SpecChecker.CoreLibrary.Common;

namespace SpecChecker.CoreLibrary.Tasks
{
	public sealed class TaskProcessor
	{
		public void Execute(string taskXmlFile, bool enableConsoleOut)
		{
			if( File.Exists(taskXmlFile) == false )
				throw new FileNotFoundException("指定的文件不存在：" + taskXmlFile);

			JobOption job = XmlHelper.XmlDeserializeFromFile<JobOption>(taskXmlFile);

			TaskContext context = new TaskContext(job, enableConsoleOut);

			if( job.Actions == null )
				return;


			ReplacePathVars(job);

			try {
				foreach( TaskAction a in job.Actions ) {
					ITask task = TaskFactory.CreateTask(a.Type);
					task.Execute(context, a);
				}
			}
			finally {
				SaveRunLog(context);
			}
		}


		private void SaveRunLog(TaskContext context)
		{
			if( context.TotalResult == null
				|| string.IsNullOrEmpty(context.TotalResult.ConsoleText) )
				return;


			try {
				string logPath = Path.Combine(Environment.CurrentDirectory, @"Log111\RunLog");
				if( Directory.Exists(logPath) == false )
					Directory.CreateDirectory(logPath);

				string logFileName = string.Format("{0}--{1}.log",
					DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), context.Branch.Id);

				string logFilePath = Path.Combine(logPath, logFileName);

				File.WriteAllText(logFilePath, context.TotalResult.ConsoleText, Encoding.UTF8);
			}
			catch {
				// 写日志文件出错就忽略异常。
			}
		}


		private void ReplacePathVars(JobOption job)
		{
			if( job.CodePath != null ) {
				for( int i = 0; i < job.CodePath.Length; i++ )
					job.CodePath[i] = PathHelper.ReplaceEnvVars(job.CodePath[i]);
			}

			if( job.SlnFiles != null ) {
				for( int i = 0; i < job.SlnFiles.Length; i++ )
					job.SlnFiles[i] = PathHelper.ReplaceEnvVars(job.SlnFiles[i]);
			}


			foreach(var action in job.Actions ) {
				if( action.Items != null ) {
					for(int i=0;i<action.Items.Length; i++ ) {
						// 替换路径中的环境变量
						action.Items[i] = PathHelper.ReplaceEnvVars(action.Items[i]);
					}
				}
			}
		}


	}
}
