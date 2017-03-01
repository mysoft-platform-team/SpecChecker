using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.CoreLibrary.Models;

namespace SpecChecker.ScanLibrary.CodeScan
{
	/// <summary>
	/// 检查公开成员的文档注释检查（三斜杠注释）
	/// </summary>
	internal class C00025RuleExecutor : IRuleExecutor
	{
		private Rule _rule;
		private List<CodeCheckResult> _list;
		private string _filePath;

		public void Execute(List<CodeCheckResult> list, Rule rule, string filePath, string[] lines)
		{
			_rule = rule;
			_list = list;
			_filePath = filePath;

			CheckDocComment(lines);
		}


		/// <summary>
		/// 检查文档注释（目前仅检查描述部分）
		/// </summary>
		/// <param name="lines"></param>
		private void CheckDocComment(string[] lines)
		{
			// 保存最近一次扫描出来的XML注释
			List<string> commentLines = new List<string>(128);
			// XML注释出现的开始行号
			int commentStartLine = 0;


			// 临时变量，指示当前行号
			int index = 0;
			foreach( string line in lines ) {
				index++;
				string currentLine = line.Trim();

				// 如果是XML注释行，就累加
				if( currentLine.StartsWith("///") ) {

					// 记住开始行号
					if( commentStartLine == 0 )
						commentStartLine = index;

					// 累加流行行（去掉开头的 三斜杠）
					commentLines.Add(currentLine.Substring(3));
				}
				else {
					// 分析XML注释块
					if( commentStartLine > 0 ) {
						CheckXmlComment(commentLines, commentStartLine);

						// 清空变量，等待下次收集XML注释行
						commentStartLine = 0;
						commentLines.Clear();
					}
				}
			}

			if( commentStartLine > 0 ) {
				CheckXmlComment(commentLines, commentStartLine);
			}
		}

		private void CheckXmlComment(List<string> commentLines, int commentStartLine)
		{
			if( commentStartLine <= 0 || commentLines.Count == 0 )
				return;

			// 全并成XML字符串
			string xml = string.Join("\n", commentLines.ToArray());
			xml = "<xml>" + xml + "</xml>"; // 重新构造一个根节点，要不然就是不合法的XML

			// XML注释中的summary的内容
			string summary = null;

			// 加载XML
			XmlDocument xmldoc = new XmlDocument();
			try {
				xmldoc.LoadXml(xml);

				// 读取summary节点
				XmlNode node = xmldoc.SelectSingleNode("//summary");
				summary = node.InnerText.Trim();
			}
			catch( Exception ex ) /* XML注释如果不能加载，就不检查了！  */ {
				string message = ex.Message;    // 方便调试时查看
				return;
			}

			// 纯粹的空注释
			if( string.IsNullOrEmpty(summary) ) {
				_list.Add(new CodeCheckResult {
					RuleCode = _rule.RuleCode,
					Reason = _rule.RuleCode + "; " + _rule.RuleName,
					LineText = commentLines[0],
					// 默认 <summary> 独占一行，所以行号加 1， 如果遇到奇葩写法，行号可能不准确了！
					LineNo = commentStartLine + 1,
					FileName = _filePath,
					BusinessUnit = BusinessUnitManager.GetNameByFilePath(_filePath)
				});

				return;
			}

			// 注释中汉字的数量
			int wordCount = CommentRule.GetWordCount(summary);

			bool isOK = false;
			if( _filePath.IndexOf(".Model") > 0 )
				isOK = wordCount >= 2;      // 允许实体的属性只包含2个汉字
			else
				isOK = wordCount >= CommentRule.LeastWordCount;      // 默认要求注释要包含5个汉字


			// summary内容没不符合规范
			if( isOK == false ) {
				_list.Add(new CodeCheckResult {
					RuleCode = _rule.RuleCode,
					Reason = _rule.RuleCode + "; " + _rule.RuleName,
					// 取第一行
					LineText = GetFirstLine(summary),
					// 默认 <summary> 独占一行，所以行号加 1， 如果遇到奇葩写法，行号可能不准确了！
					LineNo = commentStartLine + 1,
					FileName = _filePath,
					BusinessUnit = BusinessUnitManager.GetNameByFilePath(_filePath)
				});

			}
		}

		private string GetFirstLine(string text)
		{
			string[] lines = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			string line = lines[0];
			return line.Length > 120 ? line.Substring(0, 117) + "..." : line;
		}
	}
}
