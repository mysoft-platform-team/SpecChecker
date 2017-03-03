//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using SpecChecker.CoreLibrary.Models;

//namespace SpecChecker.ScanLibrary.CodeScan
//{
//	/// <summary>
//	/// 检查规则：产品代码禁止设计成静态成员，除非工具方法
//	/// </summary>
//	internal class C00005RuleExecutor : RegexRuleExecutor
//	{
//		// 其实这规则用【反射】去实现会很准确，
//		// 但是就是不容易排除，所以只好基于文本来判断了


//		/// <summary>
//		/// 检查是不是方法，属性，字段的定义格式
//		/// </summary>
//		private static readonly Regex s_regex = new Regex(@"(public|protected|private|internal)+", 
//						RegexOptions.IgnoreCase | RegexOptions.Compiled);


//		protected override bool ResultIsOk(CodeCheckResult result, string line)
//		{
//			return s_regex.IsMatch(line);
//		}
//	}
//}
