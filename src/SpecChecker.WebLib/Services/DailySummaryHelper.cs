using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Web;
using ClownFish.Base;
using SpecChecker.CoreLibrary;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.WebLib.Common;
using SpecChecker.WebLib.ViewModel;
using SpecChecker.CoreLibrary.Common;

namespace SpecChecker.WebLib.Services
{
	internal class DailySummaryHelper
	{

		private string GetDataFileName(DateTime day)
		{
			return Path.Combine(WebRuntime.Instance.GetWebSitePath(),
												"App_Data\\DailySummary",
												day.ToYMString(),
												day.ToDateString() + ".json");
		}

		/// <summary>
		/// 加载某一天的小组分类汇总数据，
		/// 如果没有指定日期的数据，返回 null
		/// </summary>
		/// <param name="day"></param>
		/// <returns></returns>
		public List<GroupDailySummary2> LoadData(DateTime day)
		{
			string filename = GetDataFileName(day);

			if( File.Exists(filename) ) {

				string json = File.ReadAllText(filename, Encoding.UTF8);

				return JsonExtensions.FromJson<List<GroupDailySummary2>>(json);
			}
			else
				return null;
		}

		/// <summary>
		/// 重新计算（按日期）所有的小组日汇总数据
		/// URL： /ajax/scan/Admin/RefreshAllDailySummary.ppx
		/// </summary>
		/// <returns></returns>
		public string RefreshAllDailySummary()
		{
			StringBuilder sb = new StringBuilder();

			DateTime day = new DateTime(2016, 3, 19);
			DateTime today = DateTime.Today;

			while(day <= today ) {
				// 拿 1 号分支判断有没有数据文件
				string filePath = ScanResultCache.GetTotalResultFilePath(1, day);
				if( File.Exists(filePath) ) {

					// 刷新第一天的汇总数据
					RefreshDailySummary(day);
					sb.AppendLine(filePath);
				}

				day = day.AddDays(1d);
			}

			return sb.ToString();
		}


		/// <summary>
		/// 重新计算某一天的小组日汇总数据
		/// URL： /ajax/scan/Admin/RefreshDailySummary.ppx?day=2016-04-28
		/// </summary>
		/// <param name="day"></param>
		/// <returns></returns>
		public string RefreshDailySummary(DateTime day)
		{
			List<GroupDailySummary2> list = new List<GroupDailySummary2>();

			foreach( BranchSettings branch in BranchManager.ConfingInstance.Branchs ) {
				// 计算当天的汇总数据
				TotalResult data = ScanResultCache.LoadTotalResult(branch.Id, day, true);

				GroupDailySummary2 summary = new GroupDailySummary2();
				summary.GroupName = branch.Name;
				summary.Data = ToSummary2(data);
                list.Add(summary);
			}

			string filename = GetDataFileName(day);

			string savepath = Path.GetDirectoryName(filename);
			if( Directory.Exists(savepath) == false )
				Directory.CreateDirectory(savepath);


			string json = list.ToJson();
			File.WriteAllText(filename, json);

            return json;
		}



		//private TotalSummary ToSummary(TotalResult data)
		//{
		//	// 如果工具还没有生成扫描数据，就直接创建一个TotalSummary实例（全部属性为零）
		//	if( data == null )
		//		return new TotalSummary();


		//	TotalSummary summary = new TotalSummary();
		//	summary.RuntimeScan = data.RuntimeScanException == null ? data.RuntimeScanResults.Count : -1;
		//	summary.DatabaseScan = data.DbCheckException == null ? data.DbCheckResults.Count : -1;
		//	summary.JsCodeScan = data.CodeCheckException == null ? data.JsCodeCheckResults.Count : -1;
		//	summary.CsCodeScan = data.CodeCheckException == null ? data.CsCodeCheckResults.Count : -1;
		//	summary.ProjectScan = data.ProjectCheckException == null ? data.ProjectCheckResults.Count : -1;
		//	summary.VsRuleScan = data.VsRuleCheckException == null ? data.VsRuleCheckResults.Count : -1;
		//          summary.PerformanceLogScan = data.PerformanceLogException == null ? data.PerformanceInfos.Count : -1;
		//          summary.ExceptionLogScan = data.ExceptionLogException == null ? data.ExceptionInfos.Count : -1;
		//          summary.UnitTestPassed  = data.UnitTestPassed;
		//	summary.UnitTestTotal = data.UnitTestTotal;
		//	summary.CodeCover = data.CodeCover;

		//	if( summary.CsCodeScan > 0 ) {
		//		// 将注释类的扫描结果独立出来，供报表显示
		//		summary.CommentScan = data.EvalCommentScanResultCount();

		//		// 扣除注释类的扫描数量
		//		summary.CsCodeScan -= summary.CommentScan;
		//	}

		//	return summary;
		//}

		private int GetIssueCount(string name, params IEnumerable<BaseScanResult>[] array)
		{
			if( array == null || array.Length == 0 )
				return 0;

			int count = 0;

			for(int i=0;i< array.Length; i++ ) {
				IEnumerable<BaseScanResult> list = array[i];
				if( list != null ) {

					count += (from x in list
							  where x.IssueCategory == name
							  select x).Count();
				}
			}

			return count;
		}

		private TotalSummary2 ToSummary2(TotalResult data)
		{
			// 如果工具还没有生成扫描数据，就直接创建一个TotalSummary实例（全部属性为零）
			if( data == null )
				return new TotalSummary2();


			IEnumerable<BaseScanResult>[] array = {
						data.RuntimeScanResults,
						//data.DbCheckResults,
						data.JsCodeCheckResults,
						data.CsCodeCheckResults,
						//data.ProjectCheckResults,
						data.VsRuleCheckResults };

			TotalSummary2 summary = new TotalSummary2();
			summary.Security = GetIssueCount("安全规则", array);
			summary.Performance = GetIssueCount("高性能规则", array);
			summary.Stability = GetIssueCount("稳定性规则", array);
			summary.Database = data.DbCheckException == null ? data.DbCheckResults.Count : 0;
			summary.Project = data.ProjectCheckException == null ? data.ProjectCheckResults.Count : 0;
			summary.ErpRule = GetIssueCount("ERP特殊规则", array);
			summary.ObjectName = GetIssueCount("命名规则", array);
			summary.Comment = GetIssueCount("注释规则", array);
			summary.VsRule = GetIssueCount("托管规则", array);
			summary.Others = GetIssueCount(IssueCategoryManager.DefaultCategory, array);



			summary.PerformanceLog = data.PerformanceLogException == null ? data.PerformanceInfos.Count : 0;
			summary.ExceptionLog = data.ExceptionLogException == null ? data.ExceptionInfos.Count : 0;
			summary.UnitTestPassed = data.UnitTestPassed;
			summary.UnitTestTotal = data.UnitTestTotal;
			summary.CodeCover = data.CodeCover;
			

			return summary;
		}


	}
}
