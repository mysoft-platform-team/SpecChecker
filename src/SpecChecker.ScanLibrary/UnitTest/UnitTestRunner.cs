using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClownFish.Base;
using ClownFish.Base.Xml;
using SpecChecker.CoreLibrary.Common;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.CoreLibrary.Models;
using SpecChecker.ScanLibrary.Tasks;

namespace SpecChecker.ScanLibrary.UnitTest
{
	public class UnitTestRunner
	{
        private static string s_nunitPath;
        private static string s_dotCoverPath;

        private TaskContext _context;
		private string _slnFilePath;
		private BranchSettings _branch;		

		private List<UnitTestResult> _unitTestResults = new List<UnitTestResult>();
		private List<UnitTestResult> _codeCoverResults = new List<UnitTestResult>();

        public UnitTestRunner(TaskContext context)
        {
			if( context == null )
				throw new ArgumentNullException(nameof(context));

			_context = context;
        }

        
        public static void CheckExePath()
		{
			string nunitPath = ConfigurationManager.AppSettings["NUnit-path"];
			string dotCoverPath = ConfigurationManager.AppSettings["dotCover-path"];

			if( string.IsNullOrEmpty(nunitPath) || string.IsNullOrEmpty(dotCoverPath) ) {
				return;
			}

			// 处理路径中可能包含的环境变量
			nunitPath = PathHelper.ReplaceEnvVars(nunitPath);
			dotCoverPath = PathHelper.ReplaceEnvVars(dotCoverPath);


			if( File.Exists(nunitPath) == false )
				throw new FileNotFoundException("文件没有找到：" + nunitPath);

			if( File.Exists(dotCoverPath) == false )
				throw new FileNotFoundException("文件没有找到：" + dotCoverPath);


			s_nunitPath = nunitPath;
			s_dotCoverPath = dotCoverPath;
		}


		public SlnUnitTestResult Execute(BranchSettings branch, string slnFilePath)
		{
			if( File.Exists(slnFilePath) == false )
				throw new FileNotFoundException("指定的文件不存在：" + slnFilePath);

			_slnFilePath = slnFilePath;
			_branch = branch;


			if( string.IsNullOrEmpty(s_nunitPath) || string.IsNullOrEmpty(s_dotCoverPath) ) {
				_context.ConsoleWrite("NUnit or ReSharper dotCover NOT INSTALL");
				return null;
			}
			

			//在 sln 文件中，类型项目，单元测试项目，WebApplication项目的GUID类型是一样，没法区开，所以就用名字来识别了。
			string[] projectFiles = SlnFileHelper.GetProjectFiles(slnFilePath)
												.Where(x => x.EndsWithIgnoreCase(".UnitTest.csproj")).ToArray();

            
            List<UnitTestWorker> threads = new List<UnitTestWorker>();

            foreach( string file in projectFiles ) {
                UnitTestWorker worker = new UnitTestWorker(_context, s_nunitPath, s_dotCoverPath, file);
                threads.Add(worker);

                // 运行每个单元测试项目
                worker.Execute();
			}


            // 等待所有线程执行
            foreach( UnitTestWorker worker in threads ) {
                worker.Wait();

                if( worker.Output.Length > 0 )
                    _context.ConsoleWrite(worker.Output.ToString());

                if( worker.Result != null )
                    _unitTestResults.Add(worker.Result);
            }

            

            //汇总统计单元测试代码覆盖率
            string mergeFile = MergedSnapshots(projectFiles);
			string reportFile = CreateCoverReport(mergeFile);
			EvalCodeCover(reportFile);


			SlnUnitTestResult total = new SlnUnitTestResult();
			total.UnitTestResults = _unitTestResults;
			total.CodeCoverResults = _codeCoverResults;
			return total;
		}

		


		private string MergedSnapshots(string[] projectFiles)
		{
			string dcvrFileName = _context.WriteTempFile("aaaa", "UnitTest-MergedSnapshots-Result.dcvr");
			MergeParams mp = new MergeParams();
			mp.Output = dcvrFileName;
			mp.Source = new List<string>();

			if( File.Exists(dcvrFileName) )
				File.Delete(dcvrFileName);

			foreach( string file in projectFiles ) {
				string dcvrFile = Path.Combine(Path.GetDirectoryName(file), "CoverResult.dcvr");
				mp.Source.Add(dcvrFile);
			}
			
			string args1File = _context.WriteTempFile(mp, "UnitTest-merge-args.xml");


			// 调用 ReSharper dotCover ，合并 代码覆盖率快照文件
			ExecuteResult result = CommandLineHelper.Execute(s_dotCoverPath, string.Format("merge \"{0}\"", args1File));
            _context.ConsoleWrite(result.ToString());

			result.FillBaseInfo();      // 设置日志的时间，当前机器名
            _context.WriteTempFile(result, "UnitTest-MergedSnapshots-Console.txt");

			System.Threading.Thread.Sleep(3000);    // 等待文件写入
			return mp.Output;
		}

		private string CreateCoverReport(string mergeFile)
		{
			string reportxmlFileName = _context.WriteTempFile("zzz", "UnitTest-CoverReport-Result.xml");
			ReportParams rp = new ReportParams();
			rp.Source = mergeFile;
			rp.Output = reportxmlFileName;
			rp.ReportType = "XML";

			if( File.Exists(reportxmlFileName) )
				File.Delete(reportxmlFileName);


			string args2File = _context.WriteTempFile(rp, "UnitTest-CoverReport-args.xml");

			// 调用 ReSharper dotCover ，生成代码覆盖率 报告文件
			ExecuteResult result = CommandLineHelper.Execute(s_dotCoverPath, string.Format("report \"{0}\"", args2File));
            _context.ConsoleWrite(result.ToString());

			result.FillBaseInfo();      // 设置日志的时间，当前机器名
            _context.WriteTempFile(result, "UnitTest-CoverReport-Console.txt");

			System.Threading.Thread.Sleep(3000);    // 等待文件写入
			return rp.Output;
		}
		

		private void EvalCodeCover(string reportFile)
		{
			if( File.Exists(reportFile) == false ) {
                _context.ConsoleWrite(reportFile + " NOT FOUND!");
				return;
			}

			List<string> asmblyNames = SlnFileHelper.GetAssemblyNames(_slnFilePath);
			ReportResult reportResult = XmlHelper.XmlDeserializeFromFile<ReportResult>(reportFile);


			if( reportResult.Assemblies != null ) {
				foreach( var a in reportResult.Assemblies ) {
					if( a.Name.EndsWith(".UnitTest", StringComparison.OrdinalIgnoreCase) )
						continue;

					// 只计算解决方案中包含的项目
					if( asmblyNames.FirstOrDefault(x => x.Equals(a.Name, StringComparison.OrdinalIgnoreCase)) == null )
						continue;

					_codeCoverResults.Add(new UnitTestResult {
						ProjectName = a.Name,
						Total = a.TotalStatements,
						Passed = a.CoveredStatements
					});

				}
			}
		}



	}
}
