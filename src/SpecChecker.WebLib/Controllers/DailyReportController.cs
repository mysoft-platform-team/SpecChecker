using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClownFish.Base;
using ClownFish.Log.Model;
using ClownFish.Web;
using SpecChecker.CoreLibrary;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.WebLib.Common;
using SpecChecker.WebLib.Services;
using SpecChecker.WebLib.ViewModel;

namespace SpecChecker.WebLib.Controllers
{
	public class DailyReportController : MyBaseController
	{
		//[OutputCache(Duration = 60 * 30, VaryByParam = "*", VaryByHeader="User-Agent")]
		[PageUrl(Url = @"/Report.phtml")]
		[PageRegexUrl(Url = @"/Report/{day:date}.phtml")]
		public IActionResult Index(DateTime? day)
		{
			// 如果URL中没有指定日期，就取当天日期去查询数据
			DateTime today = day.HasValue ? day.Value.Date : DateTime.Today;

			// 天于【今天】的日期，肯定是没有数据的
			if( today > DateTime.Today )
				return new TextResult("day is invaild.");

			// 指定的日期可能没有数据，这里会往前找，直到找到一个有数据的日期为止。
			DateTime? day1 = GetResultfulDay(today);
			if( day1.HasValue == false )
				return new TextResult("day is invaild.");

			// 同样的，去找一个有数据的日期
			DateTime? day2 = GetResultfulDay(day1.Value.AddDays(-1d));
			if( day2.HasValue == false )
				day2 = day1;

			DailyReportIndexViewModel model = new DailyReportIndexViewModel();
			model.Today = day1.Value.ToDateString();


			// 加载各小组的分类汇总数据
			DailySummaryHelper helper = new DailySummaryHelper();

			// 加载二天的数据，做【上升，下降】的趋势对比
			QaReportTableConvert convert = new QaReportTableConvert();
			convert.TodaySummary = helper.LoadData(day1.Value);
			convert.LastdaySummary = helper.LoadData(day2.Value);
			// 将数据转成表格形式
			model.QaReportTable = convert.ToTableData();

			return PageResult("/CodeScan/DailyReportIndex.cshtml", model);
		}


		[PageUrl(Url = @"/TotalReport.phtml")]
		public IActionResult TotalResport()
		{
			//return PageResult("/CodeScan/TotalReport.cshtml", null);

			// 这个页面二边是一样的，所以共用一个版本
			return new PageResult("/Views/PC/CodeScan/TotalReport.cshtml");
		}

		/// <summary>
		/// 从某天开始算起，找出一个有数据的日期，
		/// 如果参数中的日期没有数据，就往前找，直到有数据为止。
		/// </summary>
		/// <param name="today"></param>
		private DateTime? GetResultfulDay(DateTime today)
		{
			// 默认以第一个分支来做判断
			int firstBranchId = BranchManager.ConfingInstance.Branchs[0].Id;

			// 因为有时候数据不是连续的，所以如果当天数据不存在，就往前找，最后尝试100次
			for( int i = 0; i < 100; i++ ) {
				string datafile = ScanResultCache.GetTotalResultFilePath(firstBranchId, today);

				if( File.Exists(datafile) ) {
					// 只要找到数据就跳出，否则日期减一，继续往前找
					return today;
				}
				else
					today = today.AddDays(-1);
			}

			return null;
		}


		//[OutputCache(Duration = 60 * 30, VaryByParam = "*", VaryByHeader = "User-Agent")]
		[PageRegexUrl(Url = @"/DailyReport/{id}/{day:date}.phtml")]
		public IActionResult DailyReport(int id, DateTime day)
		{
			BranchSettings branch = BranchManager.GetBranch(id);
			if( branch == null )
				return new TextResult("id is invaild.");

			if( day > DateTime.Today )
				return new TextResult("day is invaild.");
			

			DateTime today =  day.Date;
			DailyReportViewModel model = new DailyReportViewModel();
			model.Today = today.ToDateString();
			model.DayMonth = today.Day.ToString() + "/" + today.Month.ToString(); //today.ToString("d/M");
			model.Branch = branch;

            // 加载工具扫描的结果
            TotalResult totalResult = null;
            try {
                totalResult = ScanResultCache.GetTotalResult(id, day);
            }
            catch( FileNotFoundException ) {
                return new TextResult("找不到匹配的数据文件。");
            }
			model.SubTotalResults = totalResult.Summary;
			model.ComplieMessage = totalResult.CompilerError;
			model.TotalResult = totalResult;

			// 注意：注释问题不是单独扫描出来的，
			// 是由于大家觉得这类问题的修复优先级可以降低点，所以就从【代码扫描结果】中提取
			// 提取之后，还要从【代码扫描结果】去掉那部分数据
			totalResult.EvalCommentScanResultCount();
			
			return PageResult("/CodeScan/DailyReport.cshtml", model);
		}


		//[OutputCache(Duration = 60 * 30, VaryByParam = "xml", VaryByHeader = "User-Agent")]
		[PageRegexUrl(Url = @"/Exception/{id}/{day:date}/{g:guid}.phtml")]
		public IActionResult ShowExceptionLog(int id, DateTime day, Guid g, int? xml)
		{
			TotalResult totalResult = ScanResultCache.GetTotalResult(id, day);
			ExceptionInfo info = totalResult.ExceptionInfos.FirstOrDefault(x => x.InfoGuid == g);

			if( info == null )
				return new TextResult("parameters is invaild.");

			if( xml.HasValue && xml.Value == 1)
				return new XmlResult(info);
			else
				return PageResult("/CodeScan/Partial/ExceptionDetail.cshtml", info);
		}


		//[OutputCache(Duration = 60 * 30, VaryByParam = "xml", VaryByHeader = "User-Agent")]
		[PageRegexUrl(Url = @"/Performance/{id}/{day:date}/{g:guid}.phtml")]
		public IActionResult ShowPerformanceLog(int id, DateTime day, Guid g, int? xml)
		{
			TotalResult totalResult = ScanResultCache.GetTotalResult(id, day);
			PerformanceInfo info = totalResult.PerformanceInfos.FirstOrDefault(x => x.InfoGuid == g);

			if( info == null )
				return new TextResult("parameters is invaild.");

			if( xml.HasValue && xml.Value == 1 )
				return new XmlResult(info);
			else
				return PageResult("/CodeScan/Partial/PerformanceDetail.cshtml", info);
		}

	
	}
}
