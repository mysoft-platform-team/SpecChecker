using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpecChecker.CoreLibrary.UnitTest
{
	[XmlRoot("Root")]
	public class ReportResult
	{
		[XmlElement("Assembly")]
		public ReportAssembly[] Assemblies { get; set; }
	}


	public class ReportAssembly
	{
		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public int CoveredStatements { get; set; }

		[XmlAttribute]
		public int TotalStatements { get; set; }

		[XmlAttribute]
		public int CoveragePercent { get; set; }
	}
}
