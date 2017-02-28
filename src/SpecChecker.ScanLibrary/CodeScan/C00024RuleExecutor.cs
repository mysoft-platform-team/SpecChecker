using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.Models;

namespace SpecChecker.ScanLibrary.CodeScan
{
	internal class C00024RuleExecutor : RegexRuleExecutor
	{
		protected override bool ResultIsOk(CodeCheckResult result, string line)
		{
			// 代码中不允许写界面文字，但是要排除以下场景
			if( result.LineText.IndexOf("[DtoDescription(") >= 0
				|| result.LineText.IndexOf("static readonly string") >= 0
				|| result.LineText.IndexOf("[ActionDescription(") >= 0
				|| result.LineText.IndexOf("[AppServiceScope(") >= 0
				|| result.LineText.IndexOf("#region ") >= 0
				|| result.LineText.IndexOf("throw new ") >= 0
				|| result.LineText.IndexOf("宋体") > 0
				|| result.LineText.IndexOf("微软雅黑") > 0
				)
				return false;

			return true;
		}
	}
}
