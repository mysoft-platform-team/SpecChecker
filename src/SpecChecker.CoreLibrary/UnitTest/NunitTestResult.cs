using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpecChecker.CoreLibrary.UnitTest
{
	[XmlRoot("test-run")]
	public class NunitTestResult
	{
		[XmlAttribute("total")]
		public int Total { get; set; }


		[XmlAttribute("passed")]
		public int Passed { get; set; }
	}
}
