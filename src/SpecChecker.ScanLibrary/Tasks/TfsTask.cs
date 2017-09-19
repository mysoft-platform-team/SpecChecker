using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.Common;
using SpecChecker.CoreLibrary.Config;

namespace SpecChecker.ScanLibrary.Tasks
{
	public sealed class TfsTask : ITask
	{
		private static string s_tfsClientPath;

		public static void CheckExePath()
		{
			string tfsClientPath = ConfigurationManager.AppSettings["tfs-client-path"];
			if( string.IsNullOrEmpty(tfsClientPath) )
				throw new ConfigurationErrorsException("没有配置 AppSettings下的tfs-client-path");


			tfsClientPath = PathHelper.ReplaceEnvVars(tfsClientPath);
			if( File.Exists(tfsClientPath) == false )
				throw new FileNotFoundException("文件没有找到：" + tfsClientPath);

			s_tfsClientPath = tfsClientPath;
		}

		public void Execute(TaskContext context, TaskAction action)
		{
			if( action.Items == null || action.Items.Length == 0 )
				action.Items = context.JobOption.CodePath;

			if( action.Items == null || action.Items.Length == 0 )
				return;


			foreach(string path in action.Items ) {
				string commandArgs = $" get  /recursive  /noprompt  itemspec \"{path}\"";
				ExecuteResult result = CommandLineHelper.Execute(s_tfsClientPath, commandArgs);
				context.ConsoleWrite(result.ToString());
			}
		}


		
	}
}
