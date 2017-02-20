using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.Common
{
	public static class PathHelper
	{
		private static readonly Regex s_regex = new Regex(@"%(\w+)%", RegexOptions.Compiled);

		/// <summary>
		/// 替换路径参数中的环境变量，返回替换后的结果
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string ReplaceEnvVars(string path)
		{
			if( string.IsNullOrEmpty(path) )
				return path;

			//Match m = s_regex.Match(path);
			return s_regex.Replace(path, MatchEvaluator);
		}

		private static string MatchEvaluator(Match m)
		{
			string name = m.Groups[1].Value;


			// 优先从配置文件中读取环境变量
			// 配置文件中的环境变量，要求以  "var-" 开头
			return System.Configuration.ConfigurationManager.AppSettings["var-" + name]
				?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);
		}


	}
}
