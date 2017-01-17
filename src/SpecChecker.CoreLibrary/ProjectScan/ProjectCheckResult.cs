using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.ProjectScan
{
	public class ProjectCheckResult : BaseScanResult
	{
		/// <summary>
		/// 项目名称
		/// </summary>
		public string ProjectName { get; set; }

		public string Configuration { get; set; }
		/// <summary>
		/// 不规范原因
		/// </summary>
		public string Reason { get; set; }

		public override string GetRuleCode()
		{
			if( this.Reason.StartsWith("SPEC:") == false )  // 早期的老数据没有定义规范编号
				return null;

			return this.Reason.Substring(0, 11);
		}


	}
}
