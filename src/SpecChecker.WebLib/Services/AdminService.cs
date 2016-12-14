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
		/// 更新 TotalResult 中的分类汇总数据
		/// URL： http://localhost:55768/ajax/scan/Admin/UpdateTotalResult.ppx
		/// </summary>
		/// <returns></returns>
		public string UpdateTotalResult()
		{
			// 各分支的数据文件数量一样多，所以只取一个分支目录下的文件即可得到所有日期
			string path = Path.Combine(WebRuntime.Instance.GetWebSitePath(), "App_Data\\ScanData");
			string[] files = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);

			foreach( string file in files ) {
				string json = File.ReadAllText(file, Encoding.UTF8);
				TotalResult totalResult = json.FromJson<TotalResult>();

				//if( totalResult.Version == "2.0" )
				//	continue;

				ScanJob job = new ScanJob();
				job.ConsoleMode = false;

				// 重算分类汇总（按扫描类别和业务单元）
				job.CalculateSubTotal(totalResult);

				if( totalResult.JsCodeCheckResults == null && totalResult.CodeCheckException == null )
					totalResult.JsCodeCheckResults = new List<CoreLibrary.CodeScan.CodeCheckResult>();

				if( totalResult.CsCodeCheckResults == null && totalResult.CodeCheckException == null )
					totalResult.CsCodeCheckResults = new List<CoreLibrary.CodeScan.CodeCheckResult>();

				totalResult.Version = "2.0";

				json = totalResult.ToJson();
				File.WriteAllText(file, json, Encoding.UTF8);
			}

			return "OK";
		}


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

		/*
				/// <summary>
				/// 升级数据格式及文件存放路径
				/// 1、数据改成ZIP包格式
				/// 2、各分支不再单独创建目录
				/// URL： http://localhost:55768/ajax/scan/Admin/UpdateFileFormat.ppx
				/// </summary>
				/// <returns></returns>
				public string UpdateFileFormat()
				{
					string root = Path.Combine(WebRuntime.Instance.GetWebSitePath(), "App_Data\\ScanData");
					string[] dirs = Directory.GetDirectories(root, "*", SearchOption.TopDirectoryOnly);

					foreach( string branchDir in dirs ) {
						DirectoryInfo dirInfo = new DirectoryInfo(branchDir);

						if( dirInfo.Name.Length == 1 ) {
							string[] subDires = Directory.GetDirectories(branchDir, "*", SearchOption.TopDirectoryOnly);

							foreach( string monthDir in subDires ) {
								DirectoryInfo info2 = new DirectoryInfo(monthDir);
								string destPath = Path.Combine(root, info2.Name);
								if( Directory.Exists(destPath) == false )
									Directory.CreateDirectory(destPath);

								string[] files = Directory.GetFiles(monthDir, "*.json", SearchOption.TopDirectoryOnly);

								foreach( string file in files ) {
									string destFileName = string.Format("{0}--{1}.zip",
															Path.GetFileNameWithoutExtension(file), dirInfo.Name);

									string destFilePath = Path.Combine(destPath, destFileName);

									ZipHelper.CreateZipFile(destFilePath, file);
								}
							}

						}
					}

					return "OK";
				}

			*/



	}
}
