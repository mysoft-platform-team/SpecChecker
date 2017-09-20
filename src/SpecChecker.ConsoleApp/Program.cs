using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ClownFish.Base;
using ClownFish.Base.Json;
using ClownFish.Log.Model;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.ScanLibrary.Tasks;
using SpecChecker.ScanLibrary.UnitTest;
using TucaoClient.Win32;

namespace SpecChecker.ConsoleApp
{
    class Program
    {
		private static bool s_enableConsoleOut = true;
        private static readonly DateTime s_startTime = DateTime.Now;
        private static string s_exeWorkingDirectory;


        static void Main(string[] args)
        {
            //Console.WriteLine("当前版本：" + JobManager.AppVersion);
            //Console.WriteLine("正在等待附加进程，，，，，，");
            //Console.ReadLine();

            // 放在计划任务中执行时，默认的当前目录是 Windows 目录，这个很坑爹！，所以这里要切到程序所在目录。
            s_exeWorkingDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            Environment.CurrentDirectory = s_exeWorkingDirectory;

            if( RunOnce.CheckApplicationIsRunning() )
                return;

            if( ProcessCommandLineArgs(args) == false )
                return;

            // 尝试删除上一次自动更新遗留的临时文件
            UpdateHelper.DeleteTempDirectory();

            if( ClientInit() == false )
                return;


            int selectBranchId = ShowMenu();
			if( selectBranchId < 0 )
				return;
            

			try {
				CheckAppSettings();
				CheckEnvironmentVariable();

				ExecuteAllJob(selectBranchId);
			}
			catch(Exception ex ) {
				ProcessException(ex);
			}

            End();			
        }


        static bool ProcessCommandLineArgs(string[] args)
        {
            if( args.Length == 0 )
                return true;

            // 检查是否启用安静模式，用于在计划任务中调用。
            if( args.FirstOrDefault(x => x == "/q") != null ) {
                s_enableConsoleOut = false;
                //return true;
            }

            string copyFlag = args.FirstOrDefault(x => x.StartsWith("/copy:"));
            if( copyFlag != null ) {
                string dest = copyFlag.Substring(6).Trim('"', ' ');
                UpdateHelper.CopyFileAndRestart(dest);
                return false;
            }

            return true;
        }


        static bool ClientInit()
        {
            DefaultJsonSerializer.SetDefaultJsonSerializerSettings += DefaultJsonSerializer_SetDefaultJsonSerializerSettings;

            try {
                // 判断是否需要自动更新
                UpdateHelper.CheckUpdate();


                // 客户端工具需要先下载所有配置文件
                ConfigHelper.ClientInit();


                return true;
            }
            catch( AutoUpdateExitException ) {
                return false;
            }
            catch( Exception ex ) {
                ProcessException(ex);
                End();
                return false;
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
			if( JobManager.Jobs.Length < 2 )
				return 0;


			if( s_enableConsoleOut && ConfigurationManager.AppSettings["AllowSelectBranchRun"] == "1" ) {
				// 显示分支，可选择只扫描一个分支，方便测试
				// 如果是计划任务模式，s_enableConsoleOut应该是false
				foreach( var job in JobManager.Jobs ) {
					Console.WriteLine("{0}: {1}", job.Id, job.Name);
				}
				Console.WriteLine("0: 全部运行");
				Console.WriteLine("输入其它非数字将会退出。");
				Console.Write("请选择要运行的分支（输入ID序号）：");

				string text = Console.ReadLine();

				int id = -1;
                if( int.TryParse(text, out id) == false )
                    return -1;

				return id;
			}

			// 如果不显示菜单就默认全部运行。
			return 0;
		}

		static void ExecuteAllJob(int selectBranchId)
		{
			string currentDirectory = Environment.CurrentDirectory;

			foreach( var job in JobManager.Jobs ) {
				if( selectBranchId > 0 && selectBranchId != job.Id )
					continue;

				// 每次都切回程序目录，防止任务执行过程完没有切回来
				Environment.CurrentDirectory = currentDirectory;

				try {
					if( s_enableConsoleOut ) {
						Console.WriteLine("\r\n\r\n=========================================================");
						Console.WriteLine("开始执行任务："+ job.Name);
						Console.WriteLine("\r\n");
					}


					// 执行每个任务
					TaskProcessor task = new TaskProcessor();
					task.Execute(job, s_enableConsoleOut);
				}
				catch( Exception ex ) {
					ProcessException(ex);
				}
			}
		}


		static void ProcessException(Exception ex)
		{
			ExceptionInfo exceptionInfo = ExceptionInfo.Create(ex);
			ClownFish.Log.LogHelper.SyncWrite(exceptionInfo);


			if( s_enableConsoleOut )
				Console.WriteLine(ex.ToString());
		}

        static void End()
        {
            if( s_enableConsoleOut ) {
                TimeSpan time = DateTime.Now - s_startTime;

                Console.WriteLine("\r\n\r\n=========================================================");
                Console.WriteLine("All Finished.");
                Console.WriteLine(time.ToString());
                Console.ReadLine();
            }
        }


    }
}
