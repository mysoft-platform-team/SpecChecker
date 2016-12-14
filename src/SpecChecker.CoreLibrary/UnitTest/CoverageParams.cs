using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.UnitTest
{
	public sealed class CoverageParams
	{
		public string TargetExecutable { get; set; }

		public string TargetArguments { get; set; }

		public string TargetWorkingDir { get; set; }

		public string Output { get; set; }
	}
}
