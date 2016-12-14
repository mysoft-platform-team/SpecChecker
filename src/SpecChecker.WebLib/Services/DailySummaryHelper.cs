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
		public List<GroupDailySummary> LoadData(DateTime day)
		{
			string filename = GetDataFileName(day);

			if( File.Exists(filename) ) {

				string json = File.ReadAllText(filename, Encoding.UTF8);

				return JsonExtensions.FromJson<List<GroupDailySummary>>(json);
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

			// 各分支的数据文件数量一样多，所以只取一个分支目录下的文件即可得到所有日期
			string path = Path.Combine(WebRuntime.Instance.GetWebSitePath(), "App_Data\\ScanData\\1");
			string[] files = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);

			foreach( string file in files ) {
				string filename = Path.GetFileNameWithoutExtension(file);
				DateTime day;
				if( DateTime.TryParse(filename, out day) ) {
					RefreshDailySummary(day);
					sb.AppendLine(file);
				}
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
			List<GroupDailySummary> list = new List<GroupDailySummary>();

			foreach( BranchSettings branch in BranchManager.ConfingInstance.Branchs ) {
				// 计算当天的汇总数据
				TotalResult data = ScanResultCache.LoadTotalResult(branch.Id, day, true);

				GroupDailySummary summary = new GroupDailySummary();
				summary.GroupName = branch.Name;
				summary.Data = ToSummary(data);
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


		private TotalSummary ToSummary(TotalResult data)
		{
			// 如果工具还没有生成扫描数据，就直接创建一个TotalSummary实例（全部属性为零）
			if( data == null )
				return new TotalSummary();


			TotalSummary summary = new TotalSummary();
			summary.RuntimeScan = data.RuntimeScanException == null ? data.RuntimeScanResults.Count : -1;
			summary.DatabaseScan = data.DbCheckException == null ? data.DbCheckResults.Count : -1;
			summary.JsCodeScan = data.CodeCheckException == null ? data.JsCodeCheckResults.Count : -1;
			summary.CsCodeScan = data.CodeCheckException == null ? data.CsCodeCheckResults.Count : -1;
			summary.ProjectScan = data.ProjectCheckException == null ? data.ProjectCheckResults.Count : -1;
			summary.VsRuleScan = data.VsRuleCheckException == null ? data.VsRuleCheckResults.Count : -1;
            summary.PerformanceLogScan = data.PerformanceLogException == null ? data.PerformanceInfos.Count : -1;
            summary.ExceptionLogScan = data.ExceptionLogException == null ? data.ExceptionInfos.Count : -1;
            summary.UnitTestPassed  = data.UnitTestPassed;
			summary.UnitTestTotal = data.UnitTestTotal;
			summary.CodeCover = data.CodeCover;
			
			if( summary.CsCodeScan > 0 ) {
				// 将注释类的扫描结果独立出来，供报表显示
				summary.CommentScan = data.EvalCommentScanResultCount();

				// 扣除注释类的扫描数量
				summary.CsCodeScan -= summary.CommentScan;
			}

			return summary;
		}

		
	}
}
