using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.CodeScan
{
	internal class ExcludeInfo
	{
		/// <summary>
		/// 规范编号
		/// </summary>
		public string SpecCode { get; set; }

		/// <summary>
		/// 相对于解决方案的文件名
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// 代码行
		/// </summary>
		public string CodeLine { get; set; }
	}
}
