using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.Models
{
	[Serializable]
	public class CodeCheckResult : BaseScanResult
	{
		///<summary>
		/// 不规范原因
		/// </summary>
		public string Reason { get; set; }

		/// <summary>
		/// 文件路径
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// 代码行号
		/// </summary>
		public int LineNo { get; set; }

		/// <summary>
		/// 不规范的代码行
		/// </summary>
		public string LineText { get; set; }


		public override string GetRuleCode()
		{
			if( this.Reason.StartsWith("SPEC:") == false )  // 早期的老数据没有定义规范编号
				return null;

			return this.Reason.Substring(0, 11);
		}
	}
}
