using System.Collections.Generic;
using ClownFish.Base.WebClient;
using SpecChecker.CoreLibrary;
using SpecChecker.CoreLibrary.Config;

namespace SpecChecker.WebLib.ViewModel
{
    public sealed class DailyReportViewModel
	{
		public string PageTitle { get; set; }

		/// <summary>
		/// 报告日期
		/// </summary>
		public string Today { get; set; }

		/// <summary>
		/// 显示在左上角的日期
		/// </summary>
		public string DayMonth { get; set; }

		/// <summary>
		/// 分支的环境信息
		/// </summary>
		public BranchSettings Branch { get; set; }

		/// <summary>
		/// 分类汇总结果（向后兼容保留，不要删除）
		/// </summary>
		public List<SubTotalResult> SubTotalResults { get; set; }


		public TotalResult TotalResult { get; set; }

		/// <summary>
		/// 编译结果消息
		/// </summary>
		public string ComplieMessage { get; set; }



		public string GetMenuLink(string dataFlag, int? page = null)
		{
			FormDataCollection form = new FormDataCollection();
			form.AddString("id", this.Branch.Id.ToString())
				.AddString("day", this.Today)
				.AddString("flag", dataFlag);

			if( page.HasValue )
				form.AddString("page", page.Value.ToString());

			return "/ajax/scan/Result/ShowScanResult.ppx?" + form.ToString();
		}
	}
}
