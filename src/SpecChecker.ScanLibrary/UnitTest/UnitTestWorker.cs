using System;
using System.Collections.Generic;
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
            _thread = new Thread(this.ThreadProc);
            _thread.IsBackground = true;
            _thread.Start();
        }


        public void Wait()
        {
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
            if( File.Exists(resultFile) )
                File.Delete(resultFile);

            string coverFile = Path.Combine(Path.GetDirectoryName(_projectFile), @"CoverResult.dcvr");
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
