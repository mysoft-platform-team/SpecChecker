using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Base.Json;
using ClownFish.Base.WebClient;
using ClownFish.Base.Xml;
using ClownFish.Log.Model;
using SpecChecker.CoreLibrary;
using SpecChecker.CoreLibrary.Common;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.CoreLibrary.Tasks;
using SpecChecker.CoreLibrary.UnitTest;
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

			//Console.Write("Press ENTER continue......");
			//Console.ReadLine();


			DateTime startTime = DateTime.Now;

			DefaultJsonSerializer.SetDefaultJsonSerializerSettings += DefaultJsonSerializer_SetDefaultJsonSerializerSettings;

			try {
				// 放在计划任务中执行时，当前目录是 Windows 目录，这个很坑爹！，所以这里要切到程序所在目录。
				string currentPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
				Environment.CurrentDirectory = currentPath;

				CheckAppSettings();
				CheckEnvironmentVariable();

				ExecuteAllJob();
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

		static void ExecuteAllJob()
		{
			string currentDirectory = Environment.CurrentDirectory;

			foreach( BranchSettings branch in BranchManager.ConfingInstance.Branchs ) {
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
				catch( WebException wex ) {
					RemoteWebException remoteWebException = new RemoteWebException(wex);
					ProcessException(remoteWebException);
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
