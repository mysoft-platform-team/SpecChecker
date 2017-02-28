using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpecChecker.ScanLibrary.CodeScan
{
	internal static class CommentRule
	{
		/// <summary>
		/// 注释中最少包含汉字的数量
		/// </summary>
		public static readonly int LeastWordCount = 6;

		/// <summary>
		/// 用于匹配汉字正则表达式
		/// </summary>
		private static Regex s_zhcnRegex = new Regex("[\u4e00-\u9fa5]", RegexOptions.Compiled);

		/// <summary>
		/// 获取一个字符串中的汉字数量
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		public static int GetWordCount(string line)
		{
			if( string.IsNullOrEmpty(line) )
				return 0;

			var matchs = s_zhcnRegex.Matches(line);

			return matchs.Count;
		}
	}
}
