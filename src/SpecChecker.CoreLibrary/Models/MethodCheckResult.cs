using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.Models
{
	public sealed class MethodCheckResult
	{
		/// <summary>
		/// 方法代码
		/// </summary>
		public MethodCodeInfo Method { get; set; }

		/// <summary>
		/// 文件路径
		/// </summary>
		public string FilePath { get; set; }

		///<summary>
		/// 不规范原因
		/// </summary>
		public string Reason { get; set; }

	}
}
