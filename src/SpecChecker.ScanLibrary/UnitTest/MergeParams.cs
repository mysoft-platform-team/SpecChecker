using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpecChecker.ScanLibrary.UnitTest
{
	public sealed class MergeParams
	{
		[XmlElement]
		public List<string> Source { get; set; }

		public string Output { get; set; }
	}
}
