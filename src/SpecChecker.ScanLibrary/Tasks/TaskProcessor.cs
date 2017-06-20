using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Base;
using ClownFish.Base.Xml;
using SpecChecker.CoreLibrary.Common;
using SpecChecker.ScanLibrary.AssemblyScan;

namespace SpecChecker.ScanLibrary.Tasks
{
	public sealed class TaskProcessor
	{
		public void Execute(string taskXmlFile, bool enableConsoleOut)
		{
			if( File.Exists(taskXmlFile) == false )
				throw new FileNotFoundException("指定的文件不存在：" + taskXmlFile);

			JobOption job = XmlHelper.XmlDeserializeFromFile<JobOption>(taskXmlFile);

            if( job.Actions == null )
                return;

            TaskContext context = new TaskContext(job, enableConsoleOut);

			// 替换路径中的环境参数
			ReplacePathVars(job);

			// 填充配置中的路径
			PaddingPath(job);

            // 记录程序的运行时环境
            LogRuntimeEnvironment(context, taskXmlFile);


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

        private void LogRuntimeEnvironment(TaskContext context, string taskXmlFile)
        {
            context.ConsoleWrite("\r\n开始时间：" + DateTime.Now.ToTimeString());
            context.ConsoleWrite("任务配置文件：");
            context.ConsoleWrite(File.ReadAllText(taskXmlFile, Encoding.UTF8));

            try {
                context.ConsoleWrite("\r\n当前计算机名：" + System.Environment.MachineName);
                context.ConsoleWrite("当前登录用户名：" + System.Environment.UserName);
            }
            catch { // 这二个信息取不到就算了
            }

            if( string.IsNullOrEmpty(context.Branch.MongoLocation) == false ) {
                MongoDB.Driver.MongoUrlBuilder urlBuilder 
                    = new MongoDB.Driver.MongoUrlBuilder(context.Branch.MongoLocation);

                if( string.IsNullOrEmpty(urlBuilder.Password) == false )
                    urlBuilder.Password = "xxxxx";

                context.ConsoleWrite("日志库地址：" + urlBuilder.ToString());
            }


            if( string.IsNullOrEmpty(context.Branch.DbLocation) == false ) {
                System.Data.SqlClient.SqlConnectionStringBuilder builder 
                    = new System.Data.SqlClient.SqlConnectionStringBuilder(context.Branch.DbLocation);

                if( string.IsNullOrEmpty(builder.Password) == false)
                    builder.Password = "xxxxx";

                context.ConsoleWrite("数据库地址：" + builder.ToString());
            }
            context.ConsoleWrite("\r\n");
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


		/// <summary>
		/// 替换路径中的环境参数
		/// </summary>
		/// <param name="job"></param>
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


		/// <summary>
		/// 填充配置中的路径
		/// </summary>
		/// <param name="job"></param>
		private void PaddingPath(JobOption job)
		{
			// 对于大多数项目来说，都有一个根目录
			// 而在具体配置文件中，同样的目录会出现在 CodePath，SlnFiles，AssemblyScan 中重复配置
			// 这样既不方便，看起来也不美观，

			// 这里允许 SlnFiles，AssemblyScan 引用 CodePath 路径，而不需要显式指定
			// 例如：CodePath ： G:\my-github\SpecChecker\demo-code\SpeChecker.DEMO
			// 那么可以这样配置    SlnFiles ： \SpeChecker.DEMO.sln


			// 只适合配置一个路径时启用
			if( job.CodePath != null && job.CodePath.Length == 1 ) {
				string basePath = job.CodePath[0].TrimEnd('\\');


				if( job.SlnFiles != null ) {
					for( int i = 0; i < job.SlnFiles.Length; i++ )
						if( job.SlnFiles[i].Length > 0 && job.SlnFiles[i][0] == '\\' ) {
							job.SlnFiles[i] = basePath + job.SlnFiles[i];
						}
				}


				foreach( var action in job.Actions ) {

					// 只处理 AssemblyScan 节点
					if( action.Type == "AssemblyScan" &&  action.Items != null ) {
						for( int i = 0; i < action.Items.Length; i++ ) {

							// 先反序列化，用于获取路径
							string line = action.Items[i];
							bool changed = false;

							TextLineSerializer serializer = new TextLineSerializer();
							AssemblyScanOption option = serializer.Deserialize<AssemblyScanOption>(line);

							if( string.IsNullOrEmpty(option.Sln) == false ) {
								if( option.Sln[0] == '\\' ) {
									option.Sln = basePath = option.Sln;
									changed = true;
								}
							}

							if( string.IsNullOrEmpty(option.Bin) == false ) {
								if( option.Bin[0] == '\\' ) {
									option.Bin = basePath + option.Bin;
									changed = true;
								}
							}

							// 如果有变化，就替换配置中的参数
							if( changed ) {
								action.Items[i] = serializer.Serialize(option);
							}
						}
					}
				}

			}
		}


	}
}
