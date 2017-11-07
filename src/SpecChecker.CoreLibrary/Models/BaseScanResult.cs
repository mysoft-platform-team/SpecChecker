using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.Models
{
	[Serializable]
	public abstract class BaseScanResult
	{

		/// <summary>
		/// 规范编号
		/// </summary>
		public string RuleCode { get; set; }


		/// <summary>
		/// 所属问题分类，
		/// </summary>
		public string IssueCategory { get; set; }


		/// <summary>
		/// 获取当前扫描结果对应的规范编号
		/// </summary>
		/// <returns></returns>
		public abstract string GetRuleCode();

	}
}
