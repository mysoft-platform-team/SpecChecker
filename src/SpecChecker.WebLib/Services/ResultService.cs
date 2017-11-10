using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ClownFish.Base;
using ClownFish.Base.Xml;
using ClownFish.Log.Model;
using ClownFish.Web;
using SpecChecker.CoreLibrary;
using SpecChecker.CoreLibrary.Models;
using SpecChecker.WebLib.Common;
using SpecChecker.WebLib.Controllers;
using SpecChecker.WebLib.ViewModel;

namespace SpecChecker.WebLib.Services
{
	public class ResultService : MyBaseController
	{
		private int _branchId;
		private string _dataFlag;
		private string _sortField;
		private int? _pageIndex;
		private TotalResult _totalResult;

		private static readonly object s_lock = new object();


		//[OutputCache(Duration = 60 * 30, VaryByParam = "id;day;flag;sort", VaryByHeader = "User-Agent")]
		public PageResult ShowScanResult(int id, DateTime day, string flag, string sort, int? page)
		{
			_totalResult = ScanResultCache.GetTotalResult(id, day);
			_branchId = id;
			_dataFlag = flag;
			_sortField = sort;
			_pageIndex = page;
			
			switch( flag ) {
				case "RuntimeScan":
					return ShowAssemblyScanResult();
	
				case "DatabaseScan":
					return ShowDatabaseScanResult();

				case "JsCodeScan":
					return ShowJsCodeScanResult();

				case "CsCodeScan":
					return ShowCsCodeScanResult();

				case "ExceptionLog":
					return ShowExceptionLog();

				case "PerformanceLog":
					return ShowPerformanceLog();

				case "ProjectScan":
					return ShowProjectScanResult();

				case "VsRuleScan":
					return ShowVsRuleScanResult();

				case "UnitTest": {
						SetLogFileUrl(id, day);
						return ShowUnitTestResult();
					}

				case "CodeCover":
					return ShowCodeCoverResult();

				case "CommentScan":
					return ShowCommentScanResult();

				default:
					throw new NotImplementedException();
			}
		}
		

		private PagedList<T> CreatePageModel<T>(List<T> list)
		{
			PagedList<T> model = new PagedList<T>(list, _pageIndex);
			model.BranchId = _branchId;
			model.DataFlag = _dataFlag;
			model.Today = _totalResult.Today;
			return model;
		}


		private PageResult PageResult<T>(string viewPath, List<T> list)
		{
			var model = CreatePageModel(list);
			return PageResult(viewPath, model);
		}
		private PageResult ShowAssemblyScanResult()
		{
			if( string.IsNullOrEmpty(_totalResult.RuntimeScanException) == false )
				return PageResult("/CodeScan/Partial/ScanFailedResult.cshtml", 
								_totalResult.RuntimeScanException);


			List<AssemblyScanResult> list = _totalResult.RuntimeScanResults;

			if( _sortField == "Message" )
				list = (from x in list orderby x.Message select x).ToList();

			else if( _sortField == "IssueCategory" )
				list = (from x in list orderby x.IssueCategory select x).ToList();


			return PageResult("/CodeScan/Partial/RuntimeScan.cshtml", list);
		}

		private PageResult ShowDatabaseScanResult()
		{
			if( string.IsNullOrEmpty(_totalResult.DbCheckException) == false )
				return PageResult("/CodeScan/Partial/ScanFailedResult.cshtml",
								_totalResult.DbCheckException);


			List<DbCheckResult> list = _totalResult.DbCheckResults;

			if( _sortField == "Reason" )
				list = (from x in list orderby x.Reason select x).ToList();

			return PageResult("/CodeScan/Partial/DatabaseScan.cshtml", list);
		}

		private PageResult ShowJsCodeScanResult()
		{
			if( string.IsNullOrEmpty(_totalResult.CodeCheckException) == false )
				return PageResult("/CodeScan/Partial/ScanFailedResult.cshtml",
								_totalResult.CodeCheckException);


			List<CodeCheckResult> list = _totalResult.JsCodeCheckResults;

			if( _sortField == "Reason" )
				list = (from x in list orderby x.Reason select x).ToList();

			else if( _sortField == "FileName" )
				list = (from x in list orderby x.FileName select x).ToList();

			else if( _sortField == "IssueCategory" )
				list = (from x in list orderby x.IssueCategory select x).ToList();

			return PageResult("/CodeScan/Partial/CodeScan.cshtml", list);
		}
		
		private PageResult ShowCsCodeScanResult()
		{
			if( string.IsNullOrEmpty(_totalResult.CodeCheckException) == false )
				return PageResult("/CodeScan/Partial/ScanFailedResult.cshtml",
								_totalResult.CodeCheckException);


			List<CodeCheckResult> list = _totalResult.GetCsCodeCheckResults();

			if( _sortField == "Reason" )
				list = (from x in list orderby x.Reason select x).ToList();

			else if( _sortField == "FileName" )
				list = (from x in list orderby x.FileName select x).ToList();

			else if( _sortField == "IssueCategory" )
				list = (from x in list orderby x.IssueCategory select x).ToList();

			return PageResult("/CodeScan/Partial/CodeScan.cshtml", list);
		}

		private PageResult ShowCommentScanResult()
		{
			if( string.IsNullOrEmpty(_totalResult.CodeCheckException) == false )
				return PageResult("/CodeScan/Partial/ScanFailedResult.cshtml",
								_totalResult.CodeCheckException);


			List<CodeCheckResult> list = _totalResult.GetCommentScanResults();

			if( _sortField == "Reason" )
				list = (from x in list orderby x.Reason select x).ToList();

			else if( _sortField == "FileName" )
				list = (from x in list orderby x.FileName select x).ToList();

			return PageResult("/CodeScan/Partial/CodeScan.cshtml", list);
		}

		private PageResult ShowProjectScanResult()
		{
			if( string.IsNullOrEmpty(_totalResult.ProjectCheckException) == false )
				return PageResult("/CodeScan/Partial/ScanFailedResult.cshtml",
								_totalResult.ProjectCheckException);


			List<ProjectCheckResult> list = _totalResult.ProjectCheckResults;

			return PageResult("/CodeScan/Partial/ProjectScan.cshtml", list);
		}
		
		private PageResult ShowVsRuleScanResult()
		{
			if( string.IsNullOrEmpty(_totalResult.VsRuleCheckException) == false )
				return PageResult("/CodeScan/Partial/ScanFailedResult.cshtml",
								_totalResult.VsRuleCheckException);


			List<VsRuleCheckResult> list = _totalResult.VsRuleCheckResults;

			return PageResult("/CodeScan/Partial/VsRuleScan.cshtml", list);
		}
		
		private PageResult ShowExceptionLog()
		{
			if( string.IsNullOrEmpty(_totalResult.ExceptionLogException) == false )
				return PageResult("/CodeScan/Partial/ScanFailedResult.cshtml",
								_totalResult.ExceptionLogException);

			List<ExceptionInfo> list = _totalResult.ExceptionInfos;

			if( _sortField == "Time" )
				list = (from x in list orderby x.Time ascending select x).ToList();

			else if( _sortField == "ExceptionType" )
				list = (from x in list orderby x.ExceptionType select x).ToList();

			ExceptionIndexViewModel model = new ExceptionIndexViewModel(list, _pageIndex);
			model.Today = _totalResult.Today;
			model.BranchId = _totalResult.Branch.Id;
			model.DataFlag = _dataFlag;

			return PageResult("/CodeScan/Partial/ExceptionList.cshtml", model);
		}

		private PageResult ShowPerformanceLog()
		{
			if( string.IsNullOrEmpty(_totalResult.PerformanceLogException) == false )
				return PageResult("/CodeScan/Partial/ScanFailedResult.cshtml",
								_totalResult.PerformanceLogException);


			List<PerformanceInfo> list = _totalResult.PerformanceInfos;

			if( _sortField == "Time" )
				list = (from x in list orderby x.Time ascending select x).ToList();

			else if( _sortField == "PerformanceType" )
				list = (from x in list orderby x.PerformanceType descending select x).ToList();

			else if( _sortField == "ExecuteTime" )
				list = (from x in list orderby x.ExecuteTime descending select x).ToList();


			PerformanceIndexViewModel model = new PerformanceIndexViewModel(list, _pageIndex);
			model.Today = _totalResult.Today;
			model.BranchId = _totalResult.Branch.Id;
			model.DataFlag = _dataFlag;

			return PageResult("/CodeScan/Partial/PerformanceList.cshtml", model);
		}

		private void SetLogFileUrl(int id, DateTime day)
		{
			// 只给显示当天的链接
			if( day.Date != DateTime.Today )
				return;

			if( _totalResult.UnitTestResults == null || _totalResult.UnitTestResults.Count == 0 )
				return;

			string today = day.Date.ToString("yyyy-MM-dd");
			string logPath = ClientLogController.GetClientLogPath();

			// 只判断第一个节点就知道前面有没有处理过
			if( _totalResult.UnitTestResults[0].LogFileUrl == null ) {
				lock( s_lock ) {
					if( _totalResult.UnitTestResults[0].LogFileUrl == null ) {
						foreach(var t in _totalResult.UnitTestResults ) {
							string name = $"{today}-{id}-UnitTest-Result-{t.ProjectName}.xml";
							string filepath = Path.Combine(logPath, name);
							if( File.Exists(filepath) )
								t.LogFileUrl = ClientLogController.GetLogFileUrl(name);
						}
					}
				}
			}
		}

		private PageResult ShowUnitTestResult()
		{
			return PageResult("/CodeScan/Partial/UnitTest.cshtml", _totalResult.UnitTestResults);
		}


		private PageResult ShowCodeCoverResult()
		{
			return PageResult("/CodeScan/Partial/CodeCover.cshtml", _totalResult.CodeCoverResults);
		}
	}
}
