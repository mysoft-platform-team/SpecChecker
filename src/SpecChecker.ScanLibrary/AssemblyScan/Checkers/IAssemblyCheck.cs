using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.Models;

namespace SpecChecker.ScanLibrary.AssemblyScan.Checkers
{
	internal interface IAssemblyCheck
	{
		void Check(Assembly asm, List<AssemblyScanResult> result);
	}
}
