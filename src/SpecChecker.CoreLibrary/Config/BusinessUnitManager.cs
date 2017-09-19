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
		public static readonly string OthersBusinessUnitName = "未知业务单元";

        // 这里先直接固定下来，如果要扩展，可以将实现类型放到配置文件中。
        private static readonly IBusinessUnitResolver s_solver = new MysoftErpBusinessUnitResolver();

        private static bool s_inited = false;
        private static readonly object s_lock = new object();
        private static BusinessUnitConfig s_config;


        private static void Init()
        {
            if( s_inited == false ) {
                lock( s_lock ) {
                    if( s_inited == false ) {
                        s_config = LoadData();

                        s_inited = true;
                    }
                }
            }
        }

        private static BusinessUnitConfig LoadData()
        {
            string xml = ConfigHelper.GetFile("SpecChecker.BusinessUnitConfig.config");
            return XmlHelper.XmlDeserialize<BusinessUnitConfig>(xml);
        }

		public static BusinessUnitConfig ConfingInstance
		{
			get {
                Init();
                return s_config;
            }
		}






		/// <summary>
		/// 根据类名获取业务单元归属名称
		/// </summary>
		/// <param name="className"></param>
		/// <returns></returns>
		public static string GetNameByClass(string className)
		{
            return s_solver.GetNameByClass(className) ?? OthersBusinessUnitName;
		}

		/// <summary>
		/// 根据URL获取业务单元归属名称
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string GetNameByUrl(string url)
		{
            return s_solver.GetNameByUrl(url) ?? OthersBusinessUnitName;
        }


		/// <summary>
		/// 根据文件路径获取业务单元归属名称
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string GetNameByFilePath(string filePath)
		{
            return s_solver.GetNameByFilePath(filePath) ?? OthersBusinessUnitName;
        }
	}
}
