using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClownFish.Base.Xml;
using SpecChecker.CoreLibrary.Common;
using SpecChecker.CoreLibrary.Models;
using SpecChecker.ScanLibrary.Tasks;

namespace SpecChecker.ScanLibrary.UnitTest
{
    internal class UnitTestWorker
    {
        private TaskContext _context;
        private string _nunitPath;
        private string _dotCoverPath;
        private string _projectFile;
        private Thread _thread;

        public UnitTestResult Result { get; private set; }

        public StringBuilder Output { get; private set; }

        public UnitTestWorker(TaskContext context, string nunitPath, string dotCoverPath, string projectFile)
        {
			if( context == null )
				throw new ArgumentNullException(nameof(context));

			_context = context;
			_nunitPath = nunitPath;
            _dotCoverPath = dotCoverPath;
            _projectFile = projectFile;

            Output = new StringBuilder(1024);
        }

        private void ConsoleWrite(string message)
        {
            this.Output.AppendLine(message);
        }

        public void Execute()
        {
            string configValue = ConfigurationManager.AppSettings["UnitTestMulitThread"] ?? "1";
            if( configValue == "1" ) {
                _thread = new Thread(this.ThreadProc);
                _thread.IsBackground = true;
                _thread.Start();
            }
            else {
                this.ThreadProc();
            }
        }


        public void Wait()
        {
            if( _thread  != null )
                _thread.Join();
        }


        private void ThreadProc()
        {
            ConsoleWrite("\r\n\r\nRun UnitTest: " + _projectFile);

            try {
                ExecuteTask();
            }
            catch(Exception ex ) {
                ConsoleWrite(ex.ToString());
            }
        }


        private void ExecuteTask()
        {
            // 单元测试结果文件，包含通过率数据
            // 为了防止项目同名，单元测试生成的结果文件和项目文件保存在一起，而不是直接保存到临时目录
            string resultFile = Path.Combine(Path.GetDirectoryName(_projectFile), @"TestResult.xml");
            string coverFile = Path.Combine(Path.GetDirectoryName(_projectFile), @"CoverResult.dcvr");

            int retryTime = 0;

            while( retryTime <= 3 ) { 

                if( File.Exists(resultFile) )
                    File.Delete(resultFile);

                if( File.Exists(coverFile) )
                    File.Delete(coverFile);


                CoverageParams cp = new CoverageParams();
                cp.TargetExecutable = _nunitPath;
                cp.TargetArguments = string.Format("\"{0}\" --result \"{1}\"  --inprocess", _projectFile, resultFile);
                cp.TargetWorkingDir = Path.Combine(Path.GetDirectoryName(_projectFile), "bin");
                cp.Output = coverFile;

                string argFileName = string.Format("UnitTest-cover-args-{0}.xml",
                                        Path.GetFileNameWithoutExtension(_projectFile).Replace(".", "_"));
                string args1File = _context.WriteTempFile(cp, argFileName);

                // 调用 ReSharper dotCover ，执行单元测试，并生成代码覆盖率快照文件
                ExecuteResult result = CommandLineHelper.Execute(_dotCoverPath, string.Format("cover \"{0}\"", args1File));
                ConsoleWrite(result.ToString());

                result.FillBaseInfo();      // 设置日志的时间，当前机器名
                _context.WriteTempFile(result, "UnitTest-RunCover-Console-" + Path.GetFileNameWithoutExtension(_projectFile) + ".txt");


                // ReSharper dotCover 时常会出现BUG，现象为不能生成代码覆盖率快照文件，例如下面的执行消息：

                //Results (nunit3) saved as D:\TFS\10_5_10_96\Prod\ERP-V1.0\40_成本系统\01_主干-开发分支\88 公共类库\Mysoft.Cbxt.Common.UnitTest\TestResult.xml
                //[JetBrains dotCover] Coverage session finished [2018/1/3 6:06:05]
                //[JetBrains dotCover] Coverage session finished but no snapshots were created.

                // 目前的解决方法是，运行 dotCover 之后再检查是否已生成快照文件，如果文件没有生成就重新执行
                // 重试的最大次数为 3

                if( File.Exists(coverFile) )
                    break;
                else {
                    System.Threading.Thread.Sleep(3000);
                    retryTime++;
                }
            }



            System.Threading.Thread.Sleep(3000);    // 等待文件写入

            if( File.Exists(resultFile) ) {
                NunitTestResult testResult = XmlHelper.XmlDeserializeFromFile<NunitTestResult>(resultFile);

                this.Result = new UnitTestResult {
                    ProjectName = Path.GetFileNameWithoutExtension(_projectFile),
                    Total = testResult.Total,
                    Passed = testResult.Passed
                };

                ConsoleWrite($"{Path.GetFileNameWithoutExtension(_projectFile)}: {testResult.Passed} / {testResult.Total}");

                // 将单元测试结果复制到临时目录。
                string tempFileName = string.Format("UnitTest-Result-{0}.xml", Path.GetFileNameWithoutExtension(_projectFile));

                try {
                    string text = File.ReadAllText(resultFile, Encoding.UTF8);
                    _context.WriteTempFile(text, tempFileName);
                }
                catch { /* 如果有异常发生，就忽略  */ }
            }
            else {
                ConsoleWrite(resultFile + " NOT FOUND!");
            }
        }

    }
}
