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
	internal sealed class RuleChecker
	{
		private Rule _rule;
		private IRuleExecutor _executor;

		public RuleChecker(Rule rule)
		{
			_rule = rule;

			if( string.IsNullOrEmpty(rule.TypeName) ) {
				_executor = new RegexRuleExecutor();
			}
			else {
				Type t = Type.GetType(rule.TypeName);
				_executor = (IRuleExecutor)Activator.CreateInstance(t);
			}
		}

		public List<CodeCheckResult> Execute(string filePath, string[] lines)
		{
			List<CodeCheckResult> list = new List<CodeCheckResult>();

			if( IsExclude(filePath) )
				return list;

			
			_executor.Execute(list, _rule, filePath, lines);

			return list;
		}

		


		private bool IsExclude(string filePath)
		{
			// 检查配置中的排除文件列表
			if( _rule.NonIncludeFiles != null && _rule.NonIncludeFiles.Length > 0 ) {
				if( _rule.NonIncludeFiles.FirstOrDefault(x => filePath.EndsWith(x)) != null )
					return true;
			}

			// 检查配置中的排除目录列表
			if( _rule.NonIncludeFolders != null && _rule.NonIncludeFolders.Length > 0 ) {
				if( _rule.NonIncludeFolders.FirstOrDefault(x => filePath.IndexOf(x) > 0) != null )
					return true;
			}

			return false;
		}
	}
}
