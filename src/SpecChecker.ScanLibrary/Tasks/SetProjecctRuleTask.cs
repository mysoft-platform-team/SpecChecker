using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.Common;

namespace SpecChecker.ScanLibrary.Tasks
{
	public sealed class SetProjecctRuleTask : ITask
	{
		internal static void ShareExecute(TaskContext context, TaskAction action, Action<string> callback)
		{
			if( action.Items == null || action.Items.Length == 0 )
				action.Items = context.JobOption.SlnFiles;

			if( action.Items == null || action.Items.Length == 0 )
				return;

			foreach( string sln in action.Items ) {
				List<string> projectFiles = SlnFileHelper.GetProjectFiles(sln);

				foreach( string file in projectFiles ) {
					string projectName = Path.GetFileNameWithoutExtension(file);
					if( projectName.EndsWith("Test") )
						continue;

					// 忽略WEB项目
					if( file.IndexOf(".Web") > 0 )
						continue;

					callback(file);
				}
			}
		}
		public void Execute(TaskContext context, TaskAction action)
		{
			ShareExecute(context, action, this.AddSet);
		}

		private void AddSet(string filePath)
		{
			//// 先备份文件
			//string bakFilePath = filePath + "._bak";

			//if( File.Exists(bakFilePath) ) {
			//	File.SetAttributes(bakFilePath, System.IO.FileAttributes.Normal);
			//	File.Delete(bakFilePath);
			//}
			//File.Copy(filePath, bakFilePath, true);

			string text = File.ReadAllText(filePath, Encoding.UTF8);

			if( text.IndexOf("<RunCodeAnalysis>true</RunCodeAnalysis>") > 0 )
				return;

			// 增加一个配置节点：<RunCodeAnalysis>
			text = text.Replace("</OutputPath>", "</OutputPath><RunCodeAnalysis>true</RunCodeAnalysis>");

			// 去掉只读属性
			File.SetAttributes(filePath, System.IO.FileAttributes.Normal);
			File.WriteAllText(filePath, text, Encoding.UTF8);
		}
	}


	public sealed class ResetProjecctRuleTask : ITask
	{
		public void Execute(TaskContext context, TaskAction action)
		{
			SetProjecctRuleTask.ShareExecute(context, action, this.RemoveSet);
		}

		private void RemoveSet(string filePath)
		{
			//// 用备份文件来恢复
			//string bakFilePath = filePath + "._bak";

			//if( File.Exists(bakFilePath) ) {
			//	// 先去只读，防止文件只读不能覆盖
			//	File.SetAttributes(filePath, System.IO.FileAttributes.Normal);

			//	File.Copy(bakFilePath, filePath, true);

			//	File.SetAttributes(bakFilePath, System.IO.FileAttributes.Normal);
			//	File.Delete(bakFilePath);
			//}

			string text = File.ReadAllText(filePath, Encoding.UTF8);

			// 删除配置节点：<RunCodeAnalysis>
			text = text.Replace("<RunCodeAnalysis>true</RunCodeAnalysis>", "");

			File.SetAttributes(filePath, System.IO.FileAttributes.Normal);
			File.WriteAllText(filePath, text, Encoding.UTF8);

			// 恢复只读属性
			File.SetAttributes(filePath, FileAttributes.Archive | FileAttributes.ReadOnly);

		}
	}
}
