using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.Models;

namespace SpecChecker.ScanLibrary.CodeScan
{
	internal class C00021RuleExecutor : RegexRuleExecutor
	{
		/// <summary>
		/// 匹配注释的正则表达式，例如：catch /* xxxxxxxxxxxx */ (Exception ex)
		/// </summary>
		private static Regex s_regex1 = new Regex(@"/\*[\w|\W]+\*/", RegexOptions.Compiled);

		/// <summary>
		/// 匹配注释的正则表达式，例如：catch(Exception ex) { // xxxxxxxxxxxx
		/// </summary>
		private static Regex s_regex2 = new Regex(@"//[\w|\W]+", RegexOptions.Compiled);


		protected override bool ResultIsOk(CodeCheckResult result, string line)
		{
			Match m = s_regex1.Match(line);
			if( m.Success ) {
				return IsKeepResult(m);
			}
			else {
				m = s_regex2.Match(line);
				if( m.Success ) {
					return IsKeepResult(m);
				}
			}

			return base.ResultIsOk(result, line);
		}


		private bool IsKeepResult(Match m)
		{
			string text = m.Groups[0].Value;
			int count = CommentRule.GetWordCount(text);
			return (count < CommentRule.LeastWordCount);  // 少于5个汉字，认为是不合格的注释，就保留结果
		}
	}
}
