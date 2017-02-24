using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.Models
{
	[Serializable]
	public sealed class AssemblyScanResult : BaseScanResult
	{
		/// <summary>
		/// 不规范的消息描述
		/// </summary>
		public string Message { get; set; }
		/// <summary>
		/// 当前类型
		/// </summary>
		public Type Type { get; set; }

		/// <summary>
		/// 类型名称，由于扫描的类型不在当前应用程序域，所以不能直接传递，所以就用字符串来传递
		/// </summary>
		public string TypeName { get; set; }
		/// <summary>
		/// 附加信息，例如定义不规范的字段名
		/// </summary>
		public string Remark { get; set; }
		/// <summary>
		/// 类型所在的DLL名称
		/// </summary>
		public string DllFileName { get; set; }

		public override string GetRuleCode()
		{
			if( this.Message.StartsWith("SPEC:") == false )  // 早期的老数据没有定义规范编号
				return null;

			//Message = "SPEC:R00028; 扩展类型必须以 Extensions 结尾"
			return this.Message.Substring(0, 11);
		}
	}
}
