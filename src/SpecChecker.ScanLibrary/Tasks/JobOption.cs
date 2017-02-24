using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpecChecker.ScanLibrary.Tasks
{
	public class JobOption
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string[] CodePath { get; set; }

		public string[] SlnFiles { get; set; }

		[XmlArrayItem("Action")]
		public List<TaskAction> Actions { get; set; }
	}

	public sealed class TaskAction
	{
		[XmlAttribute]
		public string Type { get; set; }


		[XmlElement("item")]
		public string[] Items { get; set; }
	}
}
