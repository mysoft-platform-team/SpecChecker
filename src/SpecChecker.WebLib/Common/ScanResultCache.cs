using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ClownFish.Base;
using ClownFish.Web;
using SpecChecker.CoreLibrary;
using SpecChecker.CoreLibrary.Common;
using SpecChecker.WebLib.Services;

namespace SpecChecker.WebLib.Common
{
	internal static class ScanResultCache
	{
		private static string CreateCacheKey(int id, DateTime day)
		{
			return string.Format("TotalResult_{0}_{1}", id.ToString(), day.ToDateString());
		}

		public static string GetTotalResultFilePath(int id, DateTime day)
		{
			var filename = string.Format(@"App_Data\ScanData\{0}\{1}--{2}.zip",
										day.ToYMString(), day.ToDateString(), id.ToString());

			return Path.Combine(WebRuntime.Instance.GetWebSitePath(), filename);
		}

		
		/// <summary>
		/// 直接从文件中加载数据（不经过缓存，也不添加缓存）
		/// </summary>
		/// <param name="id"></param>
		/// <param name="day"></param>
		/// <param name="ignoreFileNotExist"></param>
		/// <returns></returns>
		public static TotalResult LoadTotalResult(int id, DateTime day, bool ignoreFileNotExist)
		{
			string filename = GetTotalResultFilePath(id, day);

			if( File.Exists(filename) == false ) {
				if( ignoreFileNotExist )
					return null;
				else
					throw new FileNotFoundException("数据文件不存在：" + filename);
			}

			//TotalResult result = XmlHelper.XmlDeserializeFromFile<TotalResult>(filename);
			//string json = File.ReadAllText(filename, Encoding.UTF8);

			string json = ZipHelper.ReadTextFromZipFile(filename);
			var data = json.FromJson<TotalResult>();

			if( data.GetVersinNumber() < 3d )
				data.SetIssueCategory();

			return data;
		}


		/// <summary>
		/// 先尝试从缓存中加载数据，如果缓存不存在就从文件中加载（并添加到缓存）
		/// </summary>
		/// <param name="id"></param>
		/// <param name="day"></param>
		/// <returns></returns>
		public static TotalResult GetTotalResult(int id, DateTime day)
		{
			string cacheKey = CreateCacheKey(id, day);
			TotalResult result = HttpRuntime.Cache[cacheKey] as TotalResult;

			if( result == null ) {
				result = LoadTotalResult(id, day, false);
				HttpRuntime.Cache[cacheKey] = result;
			}

			return result;
		}

		public static void RemoveCache(int id, DateTime day)
		{
			string cacheKey = CreateCacheKey(id, day);
			HttpRuntime.Cache.Remove(cacheKey);
		}

	}
}
