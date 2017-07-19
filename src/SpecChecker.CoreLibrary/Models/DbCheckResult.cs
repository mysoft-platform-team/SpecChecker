using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.Models
{
	[Serializable]
	public class DbCheckResult : BaseScanResult
	{
		/// <summary>
		/// 不规范原因
		/// </summary>
		public string Reason { get; set; }

		/// <summary>
		/// 信息
		/// </summary>
		public string Informantion { get; set; }

		/// <summary>
		/// 表名
		/// </summary>
		public string TableName { get; set; }

        /// <summary>
        /// 对象名称
        /// </summary>
        public string ObjectName { get; set; }


		public override string GetRuleCode()
		{
			if( this.Reason.StartsWith("SPEC:") == false )	// 早期的老数据没有定义规范编号
				return null;

			return this.Reason.Substring(0, 11);
		}
	}
}
