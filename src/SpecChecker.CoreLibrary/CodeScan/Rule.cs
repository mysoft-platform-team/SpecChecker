using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.CodeScan
{
	public sealed class Rule
	{
		public string RuleCode { get; set; }
		/// <summary>
		/// 规则名称
		/// </summary>
		public string RuleName { get; set; }

		/// <summary>
		/// 正则表达式（可匹配）
		/// </summary>
		public string Regex { get; set; }

		/// <summary>
		/// 文件类型
		/// </summary>
		public string[] FileExt { get; set; }

		/// <summary>
		/// 排除的文件名
		/// </summary>
		public string[] NonIncludeFiles { get; set; }

		/// <summary>
		/// 排除的文件夹
		/// </summary>
		public string[] NonIncludeFolders { get; set; }
	}
}
