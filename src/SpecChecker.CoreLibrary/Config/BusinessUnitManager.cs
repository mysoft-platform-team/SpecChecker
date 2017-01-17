using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClownFish.Base.Files;
using ClownFish.Base.Xml;
using ClownFish.Web;

namespace SpecChecker.CoreLibrary.Config
{
	/// <summary>
	/// 管理业务单元和识别业务单元归属的工具类
	/// </summary>
	public static class BusinessUnitManager
	{
		internal static readonly string OthersBusinessUnitName = "未知业务单元";

		private static readonly FileDependencyManager<BusinessUnitConfig>
					s_config = new FileDependencyManager<BusinessUnitConfig>(		// 基于文件修改通知的缓存实例
							LoadConfig,		// 读取文件的回调委托
							GetConfigFilePath());


		public static BusinessUnitConfig ConfingInstance
		{
			get { return s_config.Result; }
		}


		private static string GetConfigFilePath()
		{
			bool isAspnetApp = string.IsNullOrEmpty(System.Web.HttpRuntime.AppDomainAppId) == false;
			if( isAspnetApp )
				return Path.Combine(WebRuntime.Instance.GetWebSitePath(), "App_Data\\config\\SpecChecker.BusinessUnitConfig.config");
			else
				return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SpecChecker.BusinessUnitConfig.config");
		}

		private static BusinessUnitConfig LoadConfig(string[] files)
		{
			BusinessUnitConfig config = XmlHelper.XmlDeserializeFromFile<BusinessUnitConfig>(files[0]);

			//foreach( BusinessUnit unit in config.List ) {
			//	for( int i = 0; i <= unit.Tables.Count - 1; i++ ) {
			//		string table = unit.Tables[i];
			//		if( table.EndsWith("*", StringComparison.Ordinal) )
			//			unit.Tables[i] = table.Replace("*", @"\w*");
			//	}
			//}

			return config;
		}


		/// <summary>
		/// 根据类名获取业务单元归属名称
		/// </summary>
		/// <param name="className"></param>
		/// <returns></returns>
		public static string GetNameByClass(string className)
		{
			if( string.IsNullOrEmpty(className) )
				return OthersBusinessUnitName;


			foreach( BusinessUnit unit in ConfingInstance.List )
				foreach( string ns in unit.Namespaces )
					if( className.StartsWith(ns, StringComparison.OrdinalIgnoreCase) )
						return unit.Name;

			return OthersBusinessUnitName;
		}
		/// <summary>
		/// 根据表名获取业务单元归属名称
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		public static string GetNameByTable(string tableName)
		{
			//if( string.IsNullOrEmpty(tableName) )
			//	return OthersBusinessUnitName;


   //         foreach ( BusinessUnit unit in ConfingInstance.List )
			//	foreach( string table in unit.Tables ) {
			//		if( table.EndsWith("*", StringComparison.Ordinal) ) {		// 加载时已将 * 替换成 \w*
			//			if( Regex.IsMatch(tableName, table, RegexOptions.IgnoreCase) )
			//				return unit.Name;
			//		}
			//		else {
			//			if( tableName.Equals(table, StringComparison.OrdinalIgnoreCase) )
			//				return unit.Name;
			//		}
			//	}

			return OthersBusinessUnitName;
        }
		/// <summary>
		/// 根据URL获取业务单元归属名称
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string GetNameByUrl(string url)
		{
			if( string.IsNullOrEmpty(url) )
				return OthersBusinessUnitName;


            // 第一轮测试，用URL前缀去匹配
            foreach ( BusinessUnit unit in ConfingInstance.List )
				foreach( string prefix in unit.UrlPrefix )
					if( url.IndexOf(prefix, StringComparison.OrdinalIgnoreCase) >= 0 )
						return unit.Name;



			// 第二轮测试，用命名空间去匹配
			foreach( BusinessUnit unit in ConfingInstance.List )
				foreach( string ns in unit.Namespaces )
					if( url.IndexOf(ns, StringComparison.OrdinalIgnoreCase) >= 0 )
						return unit.Name;


			// 第三轮测试，用项目名称去匹配
			foreach( BusinessUnit unit in ConfingInstance.List )
				if( url.IndexOf(unit.ProjectName, StringComparison.OrdinalIgnoreCase) >= 0 )
					return unit.Name;

			// 第四轮测试，用模块编号去匹配
			foreach( BusinessUnit unit in ConfingInstance.List )
				foreach( string code in unit.FunctionCodes )
					if( url.IndexOf(code, StringComparison.Ordinal) >= 0 )
						return unit.Name;

			return OthersBusinessUnitName;
        }


		/// <summary>
		/// 根据文件路径获取业务单元归属名称
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string GetNameByFilePath(string filePath)
		{
			if( string.IsNullOrEmpty(filePath) )
				return OthersBusinessUnitName;


            // 第二轮测试，用命名空间去匹配
            foreach ( BusinessUnit unit in ConfingInstance.List )
				foreach( string ns in unit.Namespaces )
					if( filePath.IndexOf(ns, StringComparison.OrdinalIgnoreCase) >= 0 )
						return unit.Name;


			// 第三轮测试，用项目名称去匹配
			foreach( BusinessUnit unit in ConfingInstance.List )
				if( filePath.IndexOf(unit.ProjectName, StringComparison.OrdinalIgnoreCase) >= 0 )
					return unit.Name;

			//if( filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) == false ) {
				// 第四轮测试，用模块编号去匹配
				foreach( BusinessUnit unit in ConfingInstance.List )
					foreach( string code in unit.FunctionCodes )
						if( filePath.IndexOf(code, StringComparison.Ordinal) >= 0 )
							return unit.Name;
			//}

			return OthersBusinessUnitName;
		}
	}
}
