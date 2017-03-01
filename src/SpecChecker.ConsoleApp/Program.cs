using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ClownFish.Base.Json;
using ClownFish.Log.Model;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.ScanLibrary.Tasks;
using SpecChecker.ScanLibrary.UnitTest;
using TucaoClient.Win32;

namespace SpecChecker.ConsoleJob
{
    class Program
    {
		private static bool s_enableConsoleOut = true;

		static void Main(string[] args)
        {
			if( RunOnce.CheckApplicationIsRunning() )
				return;


			if( args != null ) {
				// 检查是否启用安静模式，用于在计划任务中调用。
				if( args.FirstOrDefault(x => x == "/q") != null )
					s_enableConsoleOut = false;
			}

			int selectBranchId = ShowMenu();
			if( selectBranchId < 0 )
				return;


			DateTime startTime = DateTime.Now;
			DefaultJsonSerializer.SetDefaultJsonSerializerSettings += DefaultJsonSerializer_SetDefaultJsonSerializerSettings;

			try {
				// 放在计划任务中执行时，当前目录是 Windows 目录，这个很坑爹！，所以这里要切到程序所在目录。
				string currentPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
				Environment.CurrentDirectory = currentPath;

				CheckAppSettings();
				CheckEnvironmentVariable();

				ExecuteAllJob(selectBranchId);
			}
			catch(Exception ex ) {
				ProcessException(ex);
			}


			if ( s_enableConsoleOut )
            {
				TimeSpan time = DateTime.Now - startTime;

                Console.WriteLine("\r\n\r\n=========================================================");
                Console.WriteLine("All Finished.");
				Console.WriteLine(time.ToString());
                Console.ReadLine();
            }
        }


		static void CheckEnvironmentVariable()
		{
			foreach(var key in ConfigurationManager.AppSettings.AllKeys ) {
				if( key.StartsWith("var-") ) {
					string path = ConfigurationManager.AppSettings[key];

					if( System.IO.Directory.Exists(path) == false )
						throw new DirectoryNotFoundException($"环境变量{key}对应的目录不存在。");
				}
			}
		}

		static void CheckAppSettings()
		{
			TfsTask.CheckExePath();
			MsBuildTask.CheckExePath();
			UnitTestRunner.CheckExePath();
		}


		static void DefaultJsonSerializer_SetDefaultJsonSerializerSettings(object sender, SetDefaultJsonSerializerSettingArgs e)
        {
            e.Settings.Formatting = Newtonsoft.Json.Formatting.Indented;
        }

		static int ShowMenu()
		{
			// 如果只配置了一个分支，就不显示菜单了
			if( BranchManager.ConfingInstance.Branchs.Count < 2 )
				return 0;


			if( s_enableConsoleOut && ConfigurationManager.AppSettings["AllowSelectBranchRun"] == "1" ) {
				// 显示分支，可选择只扫描一个分支，方便测试
				// 如果是计划任务模式，s_enableConsoleOut应该是false
				foreach( BranchSettings branch in BranchManager.ConfingInstance.Branchs ) {
					Console.WriteLine("{0}: {1}", branch.Id, branch.Name);
				}
				Console.WriteLine("0: 全部运行");
				Console.WriteLine("输入其它非数字将会退出。");
				Console.Write("请选择要运行的分支（输入ID序号）：");

				string text = Console.ReadLine();

				int id = -1;
				int.TryParse(text, out id);
				return id;
			}

			// 如果不显示菜单就默认全部运行。
			return 0;
		}

		static void ExecuteAllJob(int selectBranchId)
		{
			string currentDirectory = Environment.CurrentDirectory;

			foreach( BranchSettings branch in BranchManager.ConfingInstance.Branchs ) {
				if( selectBranchId > 0 && selectBranchId != branch.Id )
					continue;

				// 每次都切回程序目录，防止任务执行过程完没有切回来
				Environment.CurrentDirectory = currentDirectory;

				try {
					string jobFilePath = Path.Combine(currentDirectory, $"Task-{branch.Name}.xml");


					if( s_enableConsoleOut ) {
						Console.WriteLine("\r\n\r\n=========================================================");
						Console.WriteLine("开始执行任务："+ branch.Name);
						Console.WriteLine("\r\n");
					}


					// 执行每个任务
					TaskProcessor task = new TaskProcessor();
					task.Execute(jobFilePath, s_enableConsoleOut);
				}
				catch( Exception ex ) {
					ProcessException(ex);
				}
			}
		}


		public static void ProcessException(Exception ex)
		{
			ExceptionInfo exceptionInfo = ExceptionInfo.Create(ex);
			ClownFish.Log.LogHelper.SyncWrite(exceptionInfo);


			if( s_enableConsoleOut )
				Console.WriteLine(ex.ToString());
		}

		
    }
}
