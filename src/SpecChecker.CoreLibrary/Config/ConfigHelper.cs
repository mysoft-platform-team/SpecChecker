using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.Models;

namespace SpecChecker.CoreLibrary.Config
{
	public static class ConfigHelper
	{
		public static string GetFilePath(string filename)
		{
			// 先尝试当前程序下查找
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
			if( File.Exists(path) )
				return path;

			// 如果是网站，就查找相邻的控制台项目下的BIN目录
			path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\SpecChecker.ConsoleApp\bin\" + filename);
			if( File.Exists(path) )
				return path;

			// 查找网站下的config目录
			path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"App_Data\config\" + filename);
			if( File.Exists(path) )
				return path;

			throw new FileNotFoundException($"没有找到 {filename} 文件。");
		}


        public static List<T> ExecExcludeIgnoreRules<T>(this List<T> list, BranchSettings branch) where T: BaseScanResult
        {
            if( string.IsNullOrEmpty(branch.IgnoreRules) == false ) {
                // 存在排除规则
                string[] rules = branch.IgnoreRules.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                return (from x in list
                        where rules.FirstOrDefault(r => r == x.RuleCode) == null
                        select x
                            ).ToList();
            }

            return list;
        }

    }
}
