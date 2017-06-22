using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.CoreLibrary.Models;

namespace SpecChecker.ScanLibrary.CodeScan
{
	internal class RegexRuleExecutor : IRuleExecutor
	{

		public void Execute(List<CodeCheckResult> list, Rule rule, string filePath, string[] lines)
		{
			Regex regex = new Regex(rule.Regex, RegexOptions.IgnoreCase);
			Regex regexAnd = string.IsNullOrEmpty(rule.RegexAnd) 
							? null : new Regex(rule.RegexAnd, RegexOptions.IgnoreCase);

			int index = 0;
			foreach( string line in lines ) {
				index++;

				// js 混淆后的代码
				if( line.Length > 500 && filePath.EndsWith(".js", StringComparison.OrdinalIgnoreCase) )
					continue;

				string line2 = line.Trim();
				if( line2.StartsWith("//") )        // 排除注释
					continue;


				int p = line2.IndexOf("//");        // 排除半行注释（行尾注释）
				if( p > 0 )
					line2 = line2.Substring(0, p);

				bool isMatch1 = regex.IsMatch(line2);
				bool isMatch2 = regexAnd == null ? true : regexAnd.IsMatch(line2);

				if( isMatch1 && isMatch2 ) {
					CodeCheckResult result = new CodeCheckResult {
						RuleCode = rule.RuleCode,
						Reason = rule.RuleCode + "; " + rule.RuleName,
						LineText = (line2.Length > 120 ? line2.Substring(0, 117) + "..." : line2),
						LineNo = index,
						FileName = filePath
						//BusinessUnit = BusinessUnitManager.GetNameByFilePath(filePath)
					};


					if( ResultIsOk(result, line) )
						list.Add(result);
				}
			}
		}
		

		protected virtual bool ResultIsOk(CodeCheckResult result, string line)
		{
			return true;
		}
	}
}
