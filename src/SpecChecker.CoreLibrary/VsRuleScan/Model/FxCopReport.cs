using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpecChecker.CoreLibrary.VsRuleScan.Model
{
	public class FxCopReport
	{
		public List<Target> Targets { get; set; }

		public List<Rule> Rules { get; set; }
	}

	public class Target
	{
		[XmlAttribute]
		public string Name { get; set; }

		public List<Module> Modules { get; set; }
	}

	public class Module
	{
		[XmlAttribute]
		public string Name { get; set; }

		public List<Namespace> Namespaces { get; set; }
	}

	public class Namespace
	{
		[XmlAttribute]
		public string Name { get; set; }

		public List<Type> Types { get; set; }
	}

	public class Type
	{
		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public string Kind { get; set; }


		[XmlAttribute]
		public string Accessibility { get; set; }

		[XmlAttribute]
		public string ExternallyVisible { get; set; }

		public List<Member> Members { get; set; }

		public List<Message> Messages { get; set; }
	}

	public class Member
	{
		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public string Kind { get; set; }

		[XmlAttribute]
		public string Static { get; set; }

		[XmlAttribute]
		public string Accessibility { get; set; }

		[XmlAttribute]
		public string ExternallyVisible { get; set; }

		public List<Accessor> Accessors { get; set; }

		public List<Message> Messages { get; set; }
	}


	public class Accessor
	{
		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public string Kind { get; set; }

		[XmlAttribute]
		public string Static { get; set; }

		[XmlAttribute]
		public string Accessibility { get; set; }

		[XmlAttribute]
		public string ExternallyVisible { get; set; }

		public List<Message> Messages { get; set; }

	}

	public class Message
	{
		[XmlAttribute]
		public string TypeName { get; set; }

		[XmlAttribute]
		public string Category { get; set; }

		[XmlAttribute]
		public string CheckId { get; set; }

		[XmlAttribute]
		public string Status { get; set; }

		[XmlElement("Issue")]
		public List<Issue> Issues { get; set; }
	}

	public class Issue
	{
		[XmlAttribute]
		public string Certainty { get; set; }

		[XmlAttribute]
		public string Level { get; set; }

		[XmlAttribute]
		public string Path { get; set; }

		[XmlAttribute]
		public string File { get; set; }

		[XmlAttribute]
		public string Line { get; set; }

		[XmlText]
		public string Message { get; set; }
	}


	public class Rule
	{
		[XmlAttribute]
		public string CheckId { get; set; }

		public string Url { get; set; }
	}
}
