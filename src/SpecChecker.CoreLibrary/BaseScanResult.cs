using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary
{
	[Serializable]
	public abstract class BaseScanResult
	{
		/// <summary>
		/// 业务单元
		/// </summary>
		public string BusinessUnit { get; set; }

	}
}
