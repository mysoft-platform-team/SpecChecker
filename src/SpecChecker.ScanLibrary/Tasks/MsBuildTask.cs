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

                // 注意：
                // 有些分支下面可能包含多个解决方案文件 sln，会导致编译多次
                // 这里的判断逻辑是：只要有一个项目没有编译通过，就认为是编译失败

                if( context.TotalResult.BuildIsOK.HasValue == false     // 还没有赋值过（第一次进入）
                    || context.TotalResult.BuildIsOK.Value              // 前面编译成功
                    ) {
                    string text = result.Output.Trim(' ', '\r', '\n', '\t');
                    context.TotalResult.BuildIsOK = string.IsNullOrEmpty(text);
                }
			}
		}



		
	}
}
