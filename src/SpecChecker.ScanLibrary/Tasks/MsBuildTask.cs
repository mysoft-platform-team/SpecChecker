using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.Common;

namespace SpecChecker.ScanLibrary.Tasks
{
	public sealed class MsBuildTask : ITask
	{
		private static string s_msbuildPath;

		public static void CheckExePath()
		{
			string msbuildPath = ConfigurationManager.AppSettings["msbuild-path"];
			if( string.IsNullOrEmpty(msbuildPath) )
				throw new ConfigurationErrorsException("没有配置 AppSettings下的msbuild-path");


			msbuildPath = PathHelper.ReplaceEnvVars(msbuildPath);
			if( File.Exists(msbuildPath) == false )
				throw new FileNotFoundException("文件没有找到：" + msbuildPath);

			s_msbuildPath = msbuildPath;
		}


		public void Execute(TaskContext context, TaskAction action)
		{
			if( action.Items == null || action.Items.Length == 0 )
				action.Items = context.JobOption.SlnFiles;

			if( action.Items == null || action.Items.Length == 0 ) 
				return;


			foreach( string path in action.Items ) {
				string commandArgs = $"  \"{path}\"  /t:Rebuild /p:Configuration=\"Debug\" /consoleloggerparameters:ErrorsOnly /nologo /m";
				ExecuteResult result = CommandLineHelper.Execute(s_msbuildPath, commandArgs);
				context.ConsoleWrite(result.ToString());
			}
		}



		
	}
}
