using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.Models
{
	public class UnitTestResult
	{
		/// <summary>
		/// 项目名称
		/// </summary>
		public string ProjectName { get; set; }

		/// <summary>
		/// 对应的日志文件链接
		/// </summary>
		public string LogFileUrl { get; set; }

		/// <summary>
		/// 总的用例数量或者代码块数
		/// </summary>
		public int Total { get; set; }

		/// <summary>
		/// 通过的用例数量或者覆盖的代码块数
		/// </summary>
		public int Passed { get; set; }
	}
}
