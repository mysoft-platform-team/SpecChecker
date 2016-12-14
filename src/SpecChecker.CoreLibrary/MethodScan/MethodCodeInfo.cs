using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.MethodScan
{
	public sealed class MethodCodeInfo
	{
		/// <summary>
		/// 文件路径
		/// </summary>
		public string FilePath { get; set; }

		public string NameSpace { get; set; }

		public string ClassName { get; set; }

		public string MethodName { get; set; }

		public string Body { get; set; }

		/// <summary>
		/// 代码行号
		/// </summary>
		public int LineNo { get; set; }

	}
}
