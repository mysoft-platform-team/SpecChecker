using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.Config;

namespace SpecChecker.CoreLibrary.AssemblyScan
{
	public sealed class AssemblyScaner
	{
		public List<AssemblyScanResult> Execute(BranchSettings branch, AssemblyScanOption option)
		{

			AppDomain domain = AppDomain.CreateDomain("AssemblyScan_Domain");

			try {
				ScanerProxy server = (ScanerProxy)domain.CreateInstanceAndUnwrap(
										typeof(ScanerProxy).Assembly.FullName, typeof(ScanerProxy).FullName);

				return server.Execute(branch, option);
			}
			finally {
				if( domain != null )
					AppDomain.Unload(domain);
			}
		}

	}
}
