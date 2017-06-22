using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Base.Files;
using ClownFish.Base.Xml;
using ClownFish.Web;

namespace SpecChecker.CoreLibrary.Config
{
	public static class BranchManager
	{
		private static readonly FileDependencyManager<BranchConfig>
					s_config = new FileDependencyManager<BranchConfig>(		// 基于文件修改通知的缓存实例
							LoadConfig,		// 读取文件的回调委托
							GetConfigFilePath());


		public static BranchConfig ConfingInstance
		{
			get { return s_config.Result; }
		}


		private static BranchConfig LoadConfig(string[] files)
		{
			

			BranchConfig config = XmlHelper.XmlDeserializeFromFile<BranchConfig>(files[0]);

			string defaultIgnoreRules = ConfigurationManager.AppSettings["default-IgnoreRules"];

			if( string.IsNullOrEmpty(defaultIgnoreRules) == false ) {

				if( config != null && config.Branchs != null ) {
					foreach( var b in config.Branchs ) {
						if( string.IsNullOrEmpty(b.IgnoreRules) )
							b.IgnoreRules = defaultIgnoreRules;
						else
							b.IgnoreRules = b.IgnoreRules + defaultIgnoreRules;
					}
				}
			}

			return config;
		}

		public static string GetConfigFilePath()
		{
			return ConfigHelper.GetFilePath("SpecChecker.Branchs.config");
		}

		public static BranchSettings GetBranch(int id)
		{
			BranchSettings branch = ConfingInstance.Branchs.FirstOrDefault(x => x.Id == id);
			if( branch == null )
				return ConfingInstance.Branchs[0];

			return branch;
		}
	}
}
