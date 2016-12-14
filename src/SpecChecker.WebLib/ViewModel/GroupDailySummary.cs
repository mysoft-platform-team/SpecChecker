using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.WebLib.ViewModel
{
	/// <summary>
	/// 每小组的日统计数据，当天日期用文件名来表示。
	/// </summary>
	public sealed class GroupDailySummary
	{
		/// <summary>
		/// 小组名称
		/// </summary>
		public string GroupName { get; set; }

		/// <summary>
		/// 当天的数据
		/// </summary>
		public TotalSummary Data { get; set; }
	}


	/// <summary>
	/// 所有扫描方式的数据汇总结果
	/// </summary>
	public sealed class TotalSummary
	{
		public int RuntimeScan { get; set; }

		public int DatabaseScan { get; set; }

		public int JsCodeScan { get; set; }

		public int CsCodeScan { get; set; }

		public int ProjectScan { get; set; }

		public int VsRuleScan { get; set; }
        /// <summary>
        /// 性能日志数据
        /// </summary>
        public int PerformanceLogScan { get; set; }
        /// <summary>
        /// 异常日志数据
        /// </summary>
        public int ExceptionLogScan { get; set; }
		/// <summary>
		/// 单元测试数据，通过数量
		/// </summary>
		public int UnitTestPassed { get; set; }

		/// <summary>
		/// 单元测试数据， 用例数量
		/// </summary>
		public int UnitTestTotal { get; set; }

		/// <summary>
		/// 单元测试，代码覆盖率
		/// </summary>
		public int CodeCover { get; set; }

		/// <summary>
		/// 注释扫描（从CsCodeScan中分离出来的结果）
		/// </summary>
		public int CommentScan { get; set; }

	}
}
