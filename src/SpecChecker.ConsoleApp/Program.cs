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
		private static bool s_consoleMode;

		static void Main(string[] args)
        {
			if( RunOnce.CheckApplicationIsRunning() )
				return;


			s_consoleMode = true;

			if( args != null ) {
				// 检查是否启用安静模式，用于在计划任务中调用。
				if( args.FirstOrDefault(x => x == "/q") != null )
					s_consoleMode = false;
			}

			//Console.Write("Press ENTER continue......");
			//Console.ReadLine();


			CheckAppSettings();

			DateTime startTime = DateTime.Now;

			DefaultJsonSerializer.SetDefaultJsonSerializerSettings += DefaultJsonSerializer_SetDefaultJsonSerializerSettings;


			ExecuteAllJob();
			//CreateTaskFile();


			if ( s_consoleMode )
            {
				TimeSpan time = DateTime.Now - startTime;

                Console.WriteLine("\r\n\r\n=========================================================");
                Console.WriteLine("All Finished.");
				Console.WriteLine(time.ToString());
                Console.ReadLine();
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

			try {
				foreach( BranchSettings branch in BranchManager.ConfingInstance.Branchs ) {
					// 每次都切回程序目录，防止任务执行过程完没有切回来
					Environment.CurrentDirectory = currentDirectory;

					try {
						string jobFilePath = Path.Combine(currentDirectory, $"Task-{branch.Name}.xml");

						// 执行每个任务
						TaskProcessor task = new TaskProcessor();
						task.Execute(jobFilePath);
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
			catch( Exception ex2 ) {
				ProcessException(ex2);
			}
		}


		public static void ProcessException(Exception ex)
		{
			ExceptionInfo exceptionInfo = ExceptionInfo.Create(ex);
			ClownFish.Log.LogHelper.SyncWrite(exceptionInfo);


			if( s_consoleMode )
				Console.WriteLine(ex.ToString());
		}

		static void CreateTaskFile()
		{
			JobOption job = new JobOption();
			job.Id = 1;
			job.Name = "运行平台";
			job.Actions = new List<TaskAction>();

			job.CodePath = new string[] { "aaa", "bbb"};
			job.SlnFiles = new string[] { "aaa", "bbb" };

			TaskAction a2 = new TaskAction();
			a2.Type = "ChangeWorkingDirectory";
			a2.Items = new string[] { @"%MAP%\01运行平台" };
			job.Actions.Add(a2);

			TaskAction a1 = new TaskAction();
			a1.Type = "ExecuteCommnad";
			a1.Items = new string[] { "aaaaaaaa" };
			job.Actions.Add(a1);

			

			XmlHelper.XmlSerializeToFile(job, "Task-aaaa.xml", Encoding.UTF8);
		}
    }
}
