using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.Models
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

		// 参考值：<ProjectTypeGuids>{349c5851-65df-11da-9384-00065b846f21};{fae04ec0-301f-11d3-bf4b-00c04f79efbc}</ProjectTypeGuids>
		// 对应 C#, WebApplication
		public string ProjectTypeGuids { get; set; }

		/// <summary>
		/// 项目类别，只出现在第一个PropertyGroup节点中。
		/// 目前发现已知值：
		/// Exe （Console），
		/// WinExe（WinForm）， 
		/// Library（Library or WebApplication）
		/// </summary>
		public string OutputType { get; set; }
	}

	public sealed class Project
	{
		[System.Xml.Serialization.XmlElement("PropertyGroup")]
		public PropertyGroup[] Groups { get; set; }

		public bool IsLibrary()
		{
			if( this.Groups == null || this.Groups.Length == 0 )
				return false;

			return this.Groups[0].OutputType == "Library";
		}

		public bool IsWebApplication()
		{
			if( this.IsLibrary() == false )
				return false;

			string projectTypeGuids = this.Groups[0].ProjectTypeGuids;
			if( string.IsNullOrEmpty(projectTypeGuids) )
				return false;

			return projectTypeGuids.IndexOf("349C5851-65DF-11DA-9384-00065B846F21", StringComparison.OrdinalIgnoreCase) > 0;
		}
	}
}
