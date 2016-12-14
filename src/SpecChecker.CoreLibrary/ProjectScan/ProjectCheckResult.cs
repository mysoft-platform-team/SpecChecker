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


	}
}
