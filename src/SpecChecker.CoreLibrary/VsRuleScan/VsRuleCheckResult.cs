using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.VsRuleScan
{
	public class VsRuleCheckResult : BaseScanResult
	{
		public string CheckId { get; set; }

		public string Url { get; set; }
		public string Message { get; set; }

		public string File { get; set; }

		public string Line { get; set; }


		public override string GetRuleCode()
		{
			return this.CheckId;
		}

	}
}
