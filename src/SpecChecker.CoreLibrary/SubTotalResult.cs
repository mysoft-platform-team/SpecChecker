using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary
{
	public sealed class SubTotalResult
	{
		/// <summary>
		/// 扫描检查分类
		/// </summary>
		public string ScanKind { get; set; }
		/// <summary>
		/// 业务单元分类
		/// </summary>
		public string BusinessUnit { get; set; }

		/// <summary>
		/// 问题数量
		/// </summary>
		public int Count { get; set; }
	}
}
