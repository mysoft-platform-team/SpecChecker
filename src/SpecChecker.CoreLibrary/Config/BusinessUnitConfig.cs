using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.Config
{
	/// <summary>
	/// 业务单元配置信息
	/// </summary>
	public class BusinessUnitConfig
	{
		/// <summary>
		/// 业务单元列表
		/// </summary>
		public List<BusinessUnit> List { get; set; }
	}

	/// <summary>
	/// 各业务单元配置信息
	/// </summary>
	public class BusinessUnit
	{
		/// <summary>
		/// 业务单元中文名
		/// </summary>
		[System.Xml.Serialization.XmlAttribute]
		public string Name { get; set; }

		/// <summary>
		/// 子系统名称
		/// </summary>
		[System.Xml.Serialization.XmlAttribute]
		public string SubSystem { get; set; }


		/// <summary>
		/// 项目名称（网站中的目录名）
		/// </summary>
		public string ProjectName { get; set; }
		/// <summary>
		/// 包含的命名空间
		/// </summary>
		public List<string> Namespaces { get; set; }
		/// <summary>
		/// 包含的URL地址前缀
		/// </summary>
		public List<string> UrlPrefix { get; set; }
		/// <summary>
		/// 包含的功能模块编号
		/// </summary>
		public List<string> FunctionCodes { get; set; }

		/// <summary>
		/// 包含的数据表名
		/// </summary>
		public List<string> Tables { get; set; }
	}
}
