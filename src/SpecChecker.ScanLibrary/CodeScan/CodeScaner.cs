using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using ClownFish.Base.Files;
using ClownFish.Base.Xml;
using SpecChecker.CoreLibrary.Common;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.CoreLibrary.Models;
using SpecChecker.ScanLibrary.MethodScan;

namespace SpecChecker.ScanLibrary.CodeScan
{
	public sealed class CodeScaner
	{
        #region 操作配置文件

        private static List<Rule> s_list;
        private static bool s_inited = false;
        private static readonly object s_lock = new object();

        public static void Init()
        {
            if( s_inited == false ) {
                lock( s_lock ) {
                    if( s_inited == false ) {
                        s_list = LoadConfig();
                        s_inited = true;
                    }
                }
            }
        }


		private static List<Rule> LoadConfig()
		{
            string xml = ConfigHelper.GetFile("SpecChecker.CodeRule.config");
            List<Rule> list = XmlHelper.XmlDeserialize<List<Rule>>(xml);

			foreach( Rule rule in list ) {
				// 为了方便判断，给目录名【前后】增加目录分隔符。
				if( rule.NonIncludeFolders != null && rule.NonIncludeFolders.Length > 0 ) {
					for( int i = 0; i < rule.NonIncludeFolders.Length; i++ ) {
						rule.NonIncludeFolders[i] = "\\" + rule.NonIncludeFolders[i] + "\\";
					}
				}
			}
			return list;
		}

        private static List<Rule> RuleList {
            get {
                Init();
                return s_list;
            }
        }

        #endregion

        private string _currentFilePath = null;
		private List<CodeCheckResult> _list = new List<CodeCheckResult>();

		/// <summary>
		/// 需要排除扫描的目录
		/// </summary>
		public string[] ExcludePaths { get; set; }

		private List<ExcludeInfo> LoadExcludeSettings(string srcPath)
		{
			string filePath = Path.Combine(srcPath, "exclude-specification-codescan.txt");
			if( File.Exists(filePath) == false )
				return null;

			List<ExcludeInfo> list = new List<ExcludeInfo>();
			string line = null;
			List<string> threeLines = new List<string>();

			using(StreamReader reader = new StreamReader(filePath, Encoding.UTF8) ) {
				while((line = reader.ReadLine()) != null ) {
					line = line.Trim();
					if( line.Length == 0 )
						continue;

					threeLines.Add(line);

					if( threeLines.Count == 3 ) {
						ExcludeInfo exclude = new ExcludeInfo();
						exclude.SpecCode = threeLines[0];
						exclude.FileName = threeLines[1];
						exclude.CodeLine = threeLines[2];

						threeLines.Clear();
						list.Add(exclude);
					}
				}
			}

			return list;
		}

		private void CheckExcludeRule(string srcPath)
		{
			List<ExcludeInfo> excludeRules = LoadExcludeSettings(srcPath);
			if( excludeRules == null || excludeRules.Count == 0 )
				return;
			

			foreach( CodeCheckResult result in _list ) {
				if( result.Reason == null  )		// 忽略已经排除的扫描结果
					continue;


				foreach(ExcludeInfo exclude in excludeRules ) {
					if( result.Reason.StartsWith(exclude.SpecCode)
						&& result.FileName.EndsWith(exclude.FileName)
						&& result.LineText == exclude.CodeLine) {

						result.Reason = null;       // 先做个标记，后面将会忽略这些结果
						break;
					}
				}
			}
		}
        

		public List<CodeCheckResult> Execute(BranchSettings branch, string srcPath)
		{
			// 扫描所有文件
			ScanAllFiles(srcPath);


			// 设置规范编号
			foreach( CodeCheckResult result in _list ) {
				// 早期没有这个属性，后来为了简单就统一在这里填充属性
				if( string.IsNullOrEmpty(result.RuleCode) )
					result.RuleCode = result.GetRuleCode();	
			}

			// 排除一些误报的结果
			CheckExcludeRule(srcPath);

			// 过滤有效的结果
			_list = (from x in _list
					 where x.Reason != null
					 //orderby x.BusinessUnit
					 select x).ToList();


            // 排除指定要忽略的规则
            _list = _list.ExecExcludeIgnoreRules(branch);


			// 整理文件名，去掉根目录，变成相对目录的短名
			int rootLen = srcPath.Length;
			foreach( CodeCheckResult result in _list ) 
				result.FileName = result.FileName.Substring(rootLen + 1);
			

			return _list;
		}


		private void ScanAllFiles(string path)
		{
			// 加载所有的规则扫描器
			List<RuleChecker> csCheckerList = GetRuleCheckers(".cs");
			List<RuleChecker> jsCheckerList = GetRuleCheckers(".js");
			List<RuleChecker> aspxCheckerList = GetRuleCheckers(".aspx");
            List<RuleChecker> cshtmlCheckerList = GetRuleCheckers(".cshtml");
            List<RuleChecker> configCheckerList = GetRuleCheckers(".config");


            // 这里不是一次性获取所有CS文件，避免数组太大，占用太多内存空间。
            // 而是采用先获取所有子目录，再遍历所有子目录下的CS文件。

            string[] directories = Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories);

			foreach( string dir in directories ) {
				ScanDirectory(dir, ".cs", csCheckerList);
				ScanDirectory(dir, ".aspx", aspxCheckerList);
				ScanDirectory(dir, ".js", jsCheckerList);
                ScanDirectory(dir, ".cshtml", cshtmlCheckerList);
                ScanDirectory(dir, ".config", configCheckerList);
            }
		}

		private List<RuleChecker> GetRuleCheckers(string extName)
		{
			List<Rule> rules = (from r in RuleList
								where r.FileExt != null && r.FileExt.FirstOrDefault(x => x == extName) != null
								select r
								).ToList();

			return (from r in rules
					let c = new RuleChecker(r)
					select c
					).ToList();
		}

		private bool CanSkipFile(string file, out bool isCsFile)
		{
			isCsFile = false;
			if( file.IndexOf("test", StringComparison.OrdinalIgnoreCase) >= 0 )
				return true;

			if( file.StartsWith("__", StringComparison.Ordinal) )   // 临时文件。
				return true;


			if( file.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ) {
				if( file.IndexOf("jquery") >= 0 )
					return true;

				if( file.IndexOf("\\node\\") >= 0 )
					return true;
				if( file.IndexOf("\\node_modules\\") >= 0 )
					return true;

				if( file.IndexOf("\\3rd\\") >= 0 )
					return true;

				if( file.IndexOf("\\designer\\") >= 0 )
					return true;

				if( file.IndexOf("\\packages\\") >= 0 )
					return true;

				if( file.EndsWith(".min.js", StringComparison.OrdinalIgnoreCase) )
					return true;

				if( file.EndsWith(".vsdoc.js", StringComparison.OrdinalIgnoreCase) )
					return true;
			}
			else if( file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ) {
				if( file.EndsWith("\\AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase) )
					return true;

				if( file.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase) )
					return true;

				if( file.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase) )
					return true;

				// WPF会在 obj 目录中生成一临时cs文件，用于编译
				if( file.IndexOf("obj", StringComparison.OrdinalIgnoreCase) >= 0 )
					return true;

				isCsFile = true;
			}

			// 如果有指定忽略目录的配置文件，再检查路径是否为忽略目录
			if( this.ExcludePaths != null
				&& this.ExcludePaths.FirstOrDefault(x => file.StartsWith(x, StringComparison.OrdinalIgnoreCase)) != null )
				return true;
			
			
			// 不是可忽略的文件
			return false;
		}


		private void ScanDirectory(string dir, string extName, List<RuleChecker> checkerList)
		{
			bool isCsFile = false;
			string[] files = Directory.GetFiles(dir, "*" + extName, SearchOption.TopDirectoryOnly);

			foreach( string file in files ) {

				if( CanSkipFile(file, out isCsFile) )
					continue;


				_currentFilePath = file;
				string[] lines = File.ReadAllLines(file, Encoding.UTF8);

				// 执行每个检查规则
				foreach( RuleChecker checker in checkerList )
					_list.AddRange(checker.Execute(file, lines));


				// 方法体的相关规则扫描
				if( isCsFile ) {
					MethodScaner methodScaner = new MethodScaner();
					_list.AddRange(methodScaner.Execute(file));
				}
			}
		}






	}
}
