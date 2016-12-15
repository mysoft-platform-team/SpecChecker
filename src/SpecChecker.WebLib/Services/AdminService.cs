using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Web;
using ClownFish.Base;
using SpecChecker.CoreLibrary;
using SpecChecker.WebLib.Common;
using SpecChecker.CoreLibrary.Common;
using SpecChecker.WebLib.Controllers;

namespace SpecChecker.WebLib.Services
{
	public class AdminService : MyBaseController
	{

/*
		/// <summary>
		/// 重新计算（按日期）所有的小组日汇总数据
		/// URL： http://localhost:55768/ajax/scan/Admin/RefreshAllDailySummary.ppx
		/// </summary>
		/// <returns></returns>
		public string RefreshAllDailySummary()
		{
			DailySummaryHelper helper = new DailySummaryHelper();
			return helper.RefreshAllDailySummary();
		}

	*/
	

		/// <summary>
		/// 重新计算某一天的小组日汇总数据
		/// URL： http://localhost:55768/ajax/scan/Admin/RefreshDailySummary.ppx?day=2016-04-28
		/// </summary>
		/// <param name="day"></param>
		/// <returns></returns>
		public string RefreshDailySummary(DateTime day)
		{
			DailySummaryHelper helper = new DailySummaryHelper();
			return helper.RefreshDailySummary(day);
		}


		/// <summary>
		/// 清除老的日志文件
		/// URL： http://localhost:55768/ajax/scan/Admin/ClearOldClientLog.ppx
		/// </summary>
		/// <returns></returns>
		public string ClearOldClientLog()
		{
			DeleteTxtFiles("*.xml");
			DeleteTxtFiles("*.txt");
			DeleteZipFiles();

			return "OK";
		}


		private void DeleteTxtFiles(string pattern)
		{
			// 只保留一周的日志文件
			DateTime minday = DateTime.Today.AddDays(-5d);

			string logpath = ClientLogController.GetClientLogPath();
			string[] files = Directory.GetFiles(logpath, pattern, SearchOption.TopDirectoryOnly);
			foreach( var file in files ) {
				try {
					string filename = Path.GetFileNameWithoutExtension(file);
					string dateString = filename.Substring(0, 10);
					DateTime day = DateTime.Parse(dateString);
					if( day <= minday )
						File.Delete(file);
				}
				catch {
					// 如果访问文件失败，就只能忽略异常了
				}
			}
		}


		private void DeleteZipFiles()
		{
			// ZIP文件只保留一天一个文件
			string logpath = ClientLogController.GetClientLogPath();
			Dictionary<string, string> dict = new Dictionary<string, string>();

			string[] files = Directory.GetFiles(logpath, "*.zip", SearchOption.TopDirectoryOnly);
			
			foreach( var file in files ) {
				try {
					string filename = Path.GetFileNameWithoutExtension(file);
					string key = filename.Substring(0, 12); // 分支号 + 日期
					string value = null;

					// 一天只保留一个文件
					if( dict.TryGetValue(key, out value) ) {
						File.Delete(value);
						dict[key] = file;
					}
					else {
						dict[key] = file;
					}
				}
				catch {
					// 如果访问文件失败，就只能忽略异常了
				}
			}
		}





	}
}
