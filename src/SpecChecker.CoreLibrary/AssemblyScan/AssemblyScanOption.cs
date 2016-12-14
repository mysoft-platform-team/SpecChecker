using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.AssemblyScan
{
	public sealed class AssemblyScanOption : MarshalByRefObject
	{
		public string Sln { get; set; }
		public string Bin { get; set; }
	}
}
