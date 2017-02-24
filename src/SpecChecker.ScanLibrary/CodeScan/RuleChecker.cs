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

		public RuleChecker(Rule rule)
		{
			_rule = rule;
		}

		public List<CodeCheckResult> Execute(string filePath, string[] lines)
		{
			List<CodeCheckResult> list = new List<CodeCheckResult>();

			if( IsExclude(filePath) )
				return list;

			

			Regex regex = new Regex(_rule.Regex, RegexOptions.IgnoreCase);
			
			int index = 0;
			foreach( string line in lines ) {
				index++;

				// js 混淆后的代码
				if( line.Length > 500 && filePath.EndsWith(".js", StringComparison.OrdinalIgnoreCase) )
					continue;

				string line2 = line.Trim();
				if( line2.StartsWith("//") )		// 排除注释
					continue;


				int p = line2.IndexOf("//");		// 排除半行注释（行尾注释）
				if( p > 0 )
					line2 = line2.Substring(0, p);

				if( regex.IsMatch(line2) ) {
					list.Add(new CodeCheckResult {
						Reason = _rule.RuleCode + "; " + _rule.RuleName,
						LineText = (line2.Length > 120 ? line2.Substring(0, 117) + "..." : line2) ,
						LineNo = index,
						FileName = filePath,
						BusinessUnit = BusinessUnitManager.GetNameByFilePath(filePath)
					});
				}
			}

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
