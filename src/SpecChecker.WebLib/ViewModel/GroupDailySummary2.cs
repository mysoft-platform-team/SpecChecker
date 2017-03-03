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
	public sealed class GroupDailySummary2
	{
		/// <summary>
		/// 小组名称
		/// </summary>
		public string GroupName { get; set; }

		/// <summary>
		/// 当天的数据
		/// </summary>
		public TotalSummary2 Data { get; set; }
	}


	/// <summary>
	/// 所有扫描方式的数据汇总结果
	/// </summary>
	public sealed class TotalSummary2
	{
		/// <summary>
		/// 安全规范
		/// </summary>
		public int Security { get; set; }
		/// <summary>
		/// 高性能规范
		/// </summary>
		public int Performance { get; set; }
		/// <summary>
		/// 稳定性规范
		/// </summary>
		public int Stability { get; set; }
		/// <summary>
		/// 数据库规范
		/// </summary>
		public int Database { get; set; }
		/// <summary>
		/// 项目设置规范
		/// </summary>
		public int Project { get; set; }
		/// <summary>
		/// ERP特殊规范
		/// </summary>
		public int ErpRule { get; set; }
		/// <summary>
		/// 命名规范
		/// </summary>
		public int ObjectName { get; set; }
		/// <summary>
		/// 微软托管规则
		/// </summary>
		public int VsRule { get; set; }
		/// <summary>
		/// 注释规范
		/// </summary>
		public int Comment { get; set; }

		/// <summary>
		/// 杂类规范
		/// </summary>
		public int Others { get; set; }

		/// <summary>
		/// 基础问题小计
		/// </summary>
		[System.Xml.Serialization.XmlIgnore]
		[Newtonsoft.Json.JsonIgnore]
		public int BaseTotal {
			get {
				return this.Security + this.Performance
					+ this.Stability + this.Database
					+ this.Project + this.ErpRule
					+ this.ObjectName + this.VsRule
					+ this.Others;
			}
		}

		/// <summary>
		/// 性能日志数据
		/// </summary>
		public int PerformanceLog { get; set; }
		/// <summary>
		/// 异常日志数据
		/// </summary>
		public int ExceptionLog { get; set; }
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

	}
}
