using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Base.Xml;
using ClownFish.Web;
using SpecChecker.CoreLibrary;

namespace SpecChecker.WebLib.Services
{
	/// <summary>
	/// 扫描结果的问题分类
	/// </summary>
	public class IssueCategory
	{
		[System.Xml.Serialization.XmlAttribute]
		public string Name { get; set; }
		public string[] Rules { get; set; }
	}

	internal static class IssueCategoryManager
	{
		private static bool s_inited = false;
		private static readonly object s_lock = new object();

		public static readonly string DefaultCategory = "杂类规则";


		private static List<IssueCategory> s_list;

		private static System.Collections.Concurrent.ConcurrentDictionary<string, string> s_dict
				= new System.Collections.Concurrent.ConcurrentDictionary<string, string>(
						System.Environment.ProcessorCount, 512, StringComparer.OrdinalIgnoreCase);


		//private static readonly Hashtable s_dict = new Hashtable(512, StringComparer.OrdinalIgnoreCase);


		public static List<IssueCategory> IssueCategoryList {
			get {
				Init();
				return s_list;
			}
		}


		public static string GetCategoryName(string ruleCode)
		{
			if( string.IsNullOrEmpty(ruleCode) )
				throw new ArgumentNullException(nameof(ruleCode));

			Init();

			string value = null;
			if( s_dict.TryGetValue(ruleCode, out value) )
				return value;

			//string value = s_dict[ruleCode] as string;
			//if( value != null )
			//	return value;


			if( ruleCode.Length < 11 )
				return "托管规则";

			return DefaultCategory;
		}


		private static void Init()
		{
			if( s_inited == false ) {
				lock( s_lock ) {
					if( s_inited == false ) {
						LoadData();
						s_inited = true;
					}
				}
			}
		}

		private static void LoadData()
		{
			// 加载配置文件
			string configPath = Path.Combine(WebRuntime.Instance.GetWebSitePath(), "App_Data\\config\\IssueCategory.config");
			s_list = XmlHelper.XmlDeserializeFromFile<List<IssueCategory>>(configPath);


			// 创建一个 KEY为规范编号，VALUE为规范类别 的映射表
			foreach( var cateogory in s_list ) {
				if( cateogory.Rules != null ) {
					foreach(string rule in cateogory.Rules ) {
						s_dict.TryAdd(rule, cateogory.Name);
						//s_dict.Add(rule, cateogory.Name);
					}
				}
			}
		}
	}
}
