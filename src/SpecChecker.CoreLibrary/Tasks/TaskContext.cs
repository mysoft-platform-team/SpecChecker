using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Base;
using ClownFish.Log.Model;
using SpecChecker.CoreLibrary.CodeScan;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.CoreLibrary.DbScan;
using SpecChecker.CoreLibrary.ProjectScan;
using SpecChecker.CoreLibrary.AssemblyScan;
using SpecChecker.CoreLibrary.VsRuleScan;

namespace SpecChecker.CoreLibrary.Tasks
{
	public class TaskContext
	{
		private bool _enableConsoleOut;
		private JobOption _task;
		private BranchSettings _branch;
		private string _tempPath;
		private TotalResult _totalResult;
		private StringBuilder _sb = new StringBuilder();

		public JobOption JobOption {
			get { return _task; }
		}
		public TotalResult TotalResult 
		{
			get { return _totalResult; }
		}
		public string TempPath 
		{
			get { return _tempPath; }	
		}
		public BranchSettings Branch 
		{
			get { return _branch; }
		}

		public string OutputText {
			get { return _sb.ToString(); }
		}

		public TaskContext(JobOption task, bool enableConsoleOut)
		{
			if( task == null )
				throw new ArgumentNullException(nameof(task));

			_task = task;
			_enableConsoleOut = enableConsoleOut;

			// 每个小组使用一个临时目录
			_tempPath = InitTempDirectory(task.Id);

			// 加载小组的分支配置信息
			_branch = GetBranchConfig();

			// 初始化数据结果对象
			_totalResult = InitTask();
		}

		private BranchSettings GetBranchConfig()
		{
			var branch = BranchManager.ConfingInstance.Branchs.FirstOrDefault(b => b.Id == _task.Id);
			if( branch == null )
				throw new ArgumentException("Id的值无效。");

			return branch;
		}

		public string WriteTempFile(object data, string filename)
		{
			string text = data.GetType() == typeof(string)
						? (string)data
						: data.ToXml();

			string filePath = Path.Combine(_tempPath,
					string.Format("{0}-{1}-{2}",
								DateTime.Today.ToDateString(),
								_branch.Id.ToString(),
								filename)
					);

			File.WriteAllText(filePath, text, Encoding.UTF8);

			return filePath;
		}

		public void ConsoleWrite(string message)
		{
			_sb.AppendLine(message);

			if( _enableConsoleOut )
				Console.WriteLine(message);
		}

		public void ProcessException(Exception ex)
		{
			ExceptionInfo exceptionInfo = ExceptionInfo.Create(ex);
			ClownFish.Log.LogHelper.SyncWrite(exceptionInfo);

			ConsoleWrite(ex.ToString());
		}


		private string InitTempDirectory(int jobId)
		{
			string path = Path.Combine(Environment.CurrentDirectory, $@"TempFiles\{jobId}");

			if( Directory.Exists(path) == false ) 
				Directory.CreateDirectory(path);
			

			// 清空临时目录
			FileHelper file = new FileHelper();
			file.ClearFiles(path);

			return path;
		}

		private TotalResult InitTask()
		{
			// 创建测试结果实例
			TotalResult totalResult = new TotalResult();
			totalResult.Today = DateTime.Today.ToDateString();
			totalResult.Branch = _branch;
			totalResult.Version = "3.0";


			//// 初始一些集合便于后续操作
			//totalResult.RuntimeScanResults = new List<AssemblyScanResult>();
			//totalResult.DbCheckResults = new List<DbCheckResult>();
			//totalResult.CsCodeCheckResults = new List<CodeCheckResult>();
			//totalResult.JsCodeCheckResults = new List<CodeCheckResult>();
			//totalResult.ProjectCheckResults = new List<ProjectCheckResult>();
			//totalResult.VsRuleCheckResults = new List<VsRuleCheckResult>();
			//totalResult.PerformanceInfos = new List<PerformanceInfo>();
			//totalResult.ExceptionInfos = new List<ExceptionInfo>();
			//totalResult.UnitTestResults = new List<UnitTest.UnitTestResult>();
			//totalResult.CodeCoverResults = new List<UnitTest.UnitTestResult>();
			return totalResult;
		}


		internal void SaveTotalResultToFile()
		{
			string resultFile = Path.Combine(_tempPath, "TotalResult.json");
			string json = _totalResult.ToJson();
			File.WriteAllText(resultFile, json, Encoding.UTF8);
		}
	}
}
