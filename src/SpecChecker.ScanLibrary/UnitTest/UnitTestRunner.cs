using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Base;
using ClownFish.Base.Xml;
using SpecChecker.CoreLibrary.Common;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.CoreLibrary.Models;

namespace SpecChecker.ScanLibrary.UnitTest
{
	public class UnitTestRunner
	{
		public Action<string> ConsoleWrite { get; set; }
		public Func<object, string, string> WriteTempFile{ get; set; }
		//public string TempPath { get; set; }

		private string _slnFilePath;
		private BranchSettings _branch;

		private static string s_nunitPath;
		private static string s_dotCoverPath;
		//private string _tempDir;

		private List<UnitTestResult> _unitTestResults = new List<UnitTestResult>();
		private List<UnitTestResult> _codeCoverResults = new List<UnitTestResult>();

		//private int _total = 0;
		//private int _passed = 0;
		//private int _cover = 0;

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
				ConsoleWrite("NUnit or ReSharper dotCover NOT INSTALL");
				return null;
			}
			

			//在 sln 文件中，类型项目，单元测试项目，WebApplication项目的GUID类型是一样，没法区开，所以就用名字来识别了。
			string[] projectFiles = SlnFileHelper.GetProjectFiles(slnFilePath)
												.Where(x => x.EndsWithIgnoreCase(".UnitTest.csproj")).ToArray();

			foreach( string file in projectFiles ) {
				ConsoleWrite("\r\n\r\nRun UnitTest: " + file);

				// 运行每个单元测试项目
				ExecuteUnitTestProject(file);
			}


			//汇总统计单元测试代码覆盖率
			string mergeFile = MergedSnapshots(projectFiles);
			string reportFile = CreateCoverReport(mergeFile);
			EvalCodeCover(reportFile);


			SlnUnitTestResult total = new SlnUnitTestResult();
			//total.Passed = _passed;
			//total.Total = _total;
			//total.CodeCover = _cover;
			total.UnitTestResults = _unitTestResults;
			total.CodeCoverResults = _codeCoverResults;
			return total;
		}

		private void ExecuteUnitTestProject(string file)
		{
			// 单元测试结果文件，包含通过率数据
			// 为了防止项目同名，单元测试生成的结果文件和项目文件保存在一起，而不是直接保存到临时目录
			string resultFile = Path.Combine(Path.GetDirectoryName(file), @"TestResult.xml");
			if (File.Exists(resultFile))
				File.Delete(resultFile);

			string coverFile = Path.Combine(Path.GetDirectoryName(file), @"CoverResult.dcvr");
			if (File.Exists(coverFile))
				File.Delete(coverFile);
			

			CoverageParams cp = new CoverageParams();
			cp.TargetExecutable = s_nunitPath;
			cp.TargetArguments = string.Format("\"{0}\" --result \"{1}\"  --inprocess", file, resultFile);
			cp.TargetWorkingDir = Path.Combine(Path.GetDirectoryName(file), "bin");
			cp.Output = coverFile;

			string argFileName = string.Format("UnitTest-cover-args-{0}.xml",
									Path.GetFileNameWithoutExtension(file).Replace(".", "_"));
			string args1File = WriteTempFile(cp, argFileName);

			// 调用 ReSharper dotCover ，执行单元测试，并生成代码覆盖率快照文件
			ExecuteResult result = CommandLineHelper.Execute(s_dotCoverPath, string.Format("cover \"{0}\"", args1File));
			ConsoleWrite(result.ToString());

			result.FillBaseInfo();      // 设置日志的时间，当前机器名
			//ClownFish.Log.LogHelper.SyncWrite(result);
			WriteTempFile(result, "UnitTest-RunCover-Console-" + Path.GetFileNameWithoutExtension(file) + ".txt");



			System.Threading.Thread.Sleep(3000);    // 等待文件写入
			if (File.Exists(resultFile)) {
				NunitTestResult testResult = XmlHelper.XmlDeserializeFromFile<NunitTestResult>(resultFile);

				_unitTestResults.Add(new UnitTestResult {
					ProjectName = Path.GetFileNameWithoutExtension(file),
					Total = testResult.Total,
					Passed = testResult.Passed
				});

				//_total += testResult.Total;
				//_passed += testResult.Passed;
				ConsoleWrite($"{Path.GetFileNameWithoutExtension(file)}: {testResult.Passed} / {testResult.Total}");

				// 将单元测试结果复制到临时目录。
				string tempFileName = string.Format("UnitTest-Result-{0}.xml", Path.GetFileNameWithoutExtension(file));

				try {
					string text = File.ReadAllText(resultFile, Encoding.UTF8);
					WriteTempFile(text, tempFileName);
				}
				catch { /* 如果有异常发生，就忽略  */ }
			}
			else {
				ConsoleWrite(resultFile + " NOT FOUND!");
			}
		}


		private string MergedSnapshots(string[] projectFiles)
		{
			string dcvrFileName = WriteTempFile("aaaa", "UnitTest-MergedSnapshots-Result.dcvr");
			MergeParams mp = new MergeParams();
			mp.Output = dcvrFileName;
			mp.Source = new List<string>();

			if( File.Exists(dcvrFileName) )
				File.Delete(dcvrFileName);

			foreach( string file in projectFiles ) {
				string dcvrFile = Path.Combine(Path.GetDirectoryName(file), "CoverResult.dcvr");
				mp.Source.Add(dcvrFile);
			}
			
			string args1File = WriteTempFile(mp, "UnitTest-merge-args.xml");


			// 调用 ReSharper dotCover ，合并 代码覆盖率快照文件
			ExecuteResult result = CommandLineHelper.Execute(s_dotCoverPath, string.Format("merge \"{0}\"", args1File));
			ConsoleWrite(result.ToString());

			result.FillBaseInfo();      // 设置日志的时间，当前机器名
			//ClownFish.Log.LogHelper.SyncWrite(result);
			WriteTempFile(result, "UnitTest-MergedSnapshots-Console.txt");

			System.Threading.Thread.Sleep(3000);    // 等待文件写入
			return mp.Output;
		}

		private string CreateCoverReport(string mergeFile)
		{
			string reportxmlFileName = WriteTempFile("zzz", "UnitTest-CoverReport-Result.xml");
			ReportParams rp = new ReportParams();
			rp.Source = mergeFile;
			rp.Output = reportxmlFileName;
			rp.ReportType = "XML";

			if( File.Exists(reportxmlFileName) )
				File.Delete(reportxmlFileName);


			string args2File = WriteTempFile(rp, "UnitTest-CoverReport-args.xml");

			// 调用 ReSharper dotCover ，生成代码覆盖率 报告文件
			ExecuteResult result = CommandLineHelper.Execute(s_dotCoverPath, string.Format("report \"{0}\"", args2File));
			ConsoleWrite(result.ToString());

			result.FillBaseInfo();      // 设置日志的时间，当前机器名
			//ClownFish.Log.LogHelper.SyncWrite(result);
			WriteTempFile(result, "UnitTest-CoverReport-Console.txt");

			System.Threading.Thread.Sleep(3000);    // 等待文件写入
			return rp.Output;
		}
		

		private void EvalCodeCover(string reportFile)
		{
			if( File.Exists(reportFile) == false ) {
				ConsoleWrite(reportFile + " NOT FOUND!");
				return;
			}

			List<string> asmblyNames = SlnFileHelper.GetAssemblyNames(_slnFilePath);
			ReportResult reportResult = XmlHelper.XmlDeserializeFromFile<ReportResult>(reportFile);

			//int coveredStatements = 0;
			//int totalStatements = 0;

			if( reportResult.Assemblies != null ) {
				foreach( var a in reportResult.Assemblies ) {
					//if( a.Name.StartsWith("Mysoft.", StringComparison.OrdinalIgnoreCase) == false )
					//	continue;
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

					//coveredStatements += a.CoveredStatements;
					//totalStatements += a.TotalStatements;
				}
			}

			//if( totalStatements == 0 )
			//	_cover = 0;
			//else
			//	_cover = (int)((100 * coveredStatements) / totalStatements);
		}



	}
}
