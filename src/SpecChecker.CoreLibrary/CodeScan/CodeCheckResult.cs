using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.CodeScan
{
	[Serializable]
	public class CodeCheckResult : BaseScanResult
	{
		///<summary>
		/// 不规范原因
		/// </summary>
		public string Reason { get; set; }

		/// <summary>
		/// 文件路径
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// 代码行号
		/// </summary>
		public int LineNo { get; set; }

		/// <summary>
		/// 不规范的代码行
		/// </summary>
		public string LineText { get; set; }
		
	}
}
