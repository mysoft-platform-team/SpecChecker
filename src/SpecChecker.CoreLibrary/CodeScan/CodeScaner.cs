using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClownFish.Base.Files;
using ClownFish.Base.Xml;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.CoreLibrary.MethodScan;

namespace SpecChecker.CoreLibrary.CodeScan
{
	public sealed class CodeScaner
	{
		#region 操作配置文件

		private static readonly FileDependencyManager<List<Rule>>
					s_config = new FileDependencyManager<List<Rule>>(		// 基于文件修改通知的缓存实例
							LoadConfig,		// 读取文件的回调委托
							Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SpecChecker.CodeRule.config"));


		private static List<Rule> RuleList
		{
			get { return s_config.Result; }
		}


		internal static Rule Rule25;
		internal static Regex Rule25Regex;


		private static List<Rule> LoadConfig(string[] files)
		{
			List<Rule> list = XmlHelper.XmlDeserializeFromFile<List<Rule>>(files[0]);

			foreach( Rule rule in list ) {
				// 为了方便判断，给目录名【前后】增加目录分隔符。
				if( rule.NonIncludeFolders != null && rule.NonIncludeFolders.Length > 0 ) {
					for( int i = 0; i < rule.NonIncludeFolders.Length; i++ ) {
						rule.NonIncludeFolders[i] = "\\" + rule.NonIncludeFolders[i] + "\\";
					}
				}
			}

			// 这个规则比较特殊，需要单独检查，可参考调用处
			Rule25 = list.FirstOrDefault(x => x.RuleCode == "SPEC:C00025");
			if( Rule25 != null ) {
				Rule25Regex = new Regex(Rule25.Regex, RegexOptions.IgnoreCase);

				// 排除这个特殊的规则，避免重复执行
				list = (from x in list where x.RuleCode != "SPEC:C00025" select x).ToList();
			}

			return list;
		}

		#endregion

		
		private List<CodeCheckResult> _list = new List<CodeCheckResult>();

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


		/// <summary>
		/// 特别排除 SPEC:C00024 规则的部分结果
		/// </summary>
		private void ExcludeC00024()
		{
			// 下面这些地方是需要汉字描述的。
			foreach( CodeCheckResult result in _list ) {
				if( result.Reason.StartsWith("SPEC:C00024") ) {
					// 代码中不允许写界面文字
					if( result.LineText.IndexOf("[DtoDescription(") >= 0
						|| result.LineText.IndexOf("static readonly string") >= 0
						|| result.LineText.IndexOf("[ActionDescription(") >= 0
						|| result.LineText.IndexOf("[AppServiceScope(") >= 0
						|| result.LineText.IndexOf("#region ") >= 0
						|| result.LineText.IndexOf("throw new ") >= 0
						|| result.LineText.IndexOf("宋体") > 0
						|| result.LineText.IndexOf("微软雅黑") > 0
						)
						result.Reason = null;       // 先做个标记，后面将会忽略这些结果
				}
			}
		}


		private void ExcludeIgnoreRules(BranchSettings branch)
		{
			if( string.IsNullOrEmpty(branch.IgnoreRules) == false ) {
				// 存在排除规则
				string[] rules = branch.IgnoreRules.Split(';');
				_list = (from x in _list
						let rulecode = x.Reason.Substring(0, 11)    // 11位的编号
						where rules.FirstOrDefault(r => r == rulecode) == null
						select x
							).ToList();
			}
		}

		public List<CodeCheckResult> Execute(BranchSettings branch, string srcPath)
		{
			// 扫描所有文件
			ScanAllFiles(srcPath);

			//特别排除 SPEC:C00024 规则的部分结果
			ExcludeC00024();
			
			// 排除一些误报的结果
			CheckExcludeRule(srcPath);

			// 过滤有效的结果
			_list = (from x in _list
					 where x.Reason != null
					 orderby x.BusinessUnit
					 select x).ToList();


			// 排除指定要忽略的规则
			ExcludeIgnoreRules(branch);

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


			// 这里不是一次性获取所有CS文件，避免数组太大，占用太多内存空间。
			// 而是采用先获取所有子目录，再遍历所有子目录下的CS文件。

			string[] directories = Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories);

			foreach( string dir in directories ) {
				ScanDirectory(dir, ".cs", csCheckerList);
				ScanDirectory(dir, ".aspx", aspxCheckerList);
				ScanDirectory(dir, ".js", jsCheckerList);
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

				// WPF会在 obj 目录中生成一临时cs文件，用于编译
				if( file.IndexOf("obj", StringComparison.OrdinalIgnoreCase) >= 0 )
					return true;

				isCsFile = true;
			}
			
			
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

				ScanFile(file, checkerList);


				if( isCsFile ) {
					// 方法的相关规则扫描
					MethodScaner methodScaner = new MethodScaner();
					_list.AddRange(methodScaner.Execute(file));
				}
			}
		}


		private void ScanFile(string file, List<RuleChecker> checkerList)
		{
			string[] lines = File.ReadAllLines(file, Encoding.UTF8);

			foreach(RuleChecker checker in checkerList)
				_list.AddRange(checker.Execute(file, lines));

			// 增加：检查文档注释
			if( file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) )
				_list.AddRange(CheckDocComment(file, lines));
		}

		/// <summary>
		/// 检查文档注释（目前仅检查描述部分）
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="lines"></param>
		/// <returns></returns>
		private List<CodeCheckResult> CheckDocComment(string filePath, string[] lines)
		{
			List<CodeCheckResult> list = new List<CodeCheckResult>();

			// 是否进入文档注释的描述区域
			bool inSummaryComment = false;

			int index = 0;
			foreach( string line in lines ) {
				index++;

				if( line.IndexOf("/// <summary>") >= 0 ) {
					inSummaryComment = true;
					continue;
				}

				if( line.IndexOf("/// </summary>") >= 0 ) {
					inSummaryComment = false;
					continue;
				}

				// 判断空的【文档注释】，三个斜线开头的
				if( inSummaryComment && CodeScaner.Rule25Regex != null ) {
					if( CodeScaner.Rule25Regex.IsMatch(line) ) {
						list.Add(new CodeCheckResult {
							Reason = CodeScaner.Rule25.RuleCode + "; " + CodeScaner.Rule25.RuleName,
							LineText = (line.Length > 120 ? line.Substring(0, 117) + "..." : line),
							LineNo = index,
							FileName = filePath,
							BusinessUnit = BusinessUnitManager.GetNameByFilePath(filePath)
						});

						continue;
					}
				}
			}

			return list;
		}
	}
}
