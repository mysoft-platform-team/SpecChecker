using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.Models;

namespace SpecChecker.ScanLibrary.CodeScan
{
	/// <summary>
	/// 代码扫描的执行接口定义
	/// </summary>
	public interface IRuleExecutor
	{
		/// <summary>
		/// 执行扫描过程
		/// </summary>
		/// <param name="list">扫描结果的输出列表</param>
		/// <param name="rule">当前规则对象的引用</param>
		/// <param name="filePath">当前要扫描的文件路径</param>
		/// <param name="lines">文件的所有文本行数组</param>
		void Execute(List<CodeCheckResult> list, Rule rule, string filePath, string[] lines);
	}
}
