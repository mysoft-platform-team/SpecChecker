using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.ProjectScan.Model
{
	public sealed class PropertyGroup
	{
		[System.Xml.Serialization.XmlAttribute]
		public string Condition { get; set; }

		public string OutputPath { get; set; }

		public string DocumentationFile { get; set; }

		public string TreatWarningsAsErrors { get; set; }

		public string Configuration { get; set; }
		public string AssemblyName { get; set; }
	}

	public sealed class Project
	{
		[System.Xml.Serialization.XmlElement("PropertyGroup")]
		public PropertyGroup[] Groups { get; set; }
	}
}
