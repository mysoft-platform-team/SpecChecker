using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Web;
using SpecChecker.CoreLibrary.Common;
using SpecChecker.WebLib.Common;

namespace SpecChecker.WebLib.Controllers
{
	public class ClientLogController : MyBaseController
	{
		internal static string GetClientLogPath()
		{
			string logpath = Path.Combine(WebRuntime.Instance.GetWebSitePath(), @"App_Data\ScanData\ClientLog");
			if( Directory.Exists(logpath) == false )
				Directory.CreateDirectory(logpath);

			return logpath;
		}


		[PageUrl(Url = @"/client-log.phtml")]
		public IActionResult Index(string file)
		{
			if( string.IsNullOrEmpty(file) )
				return ShowFileList();
			else
				return ShowFileText(file);
		}


		private IActionResult ShowFileList()
		{
			Dictionary<string, string> dict = new Dictionary<string, string>();

			string logpath = ClientLogController.GetClientLogPath();
			string[] allfiles = Directory.GetFiles(logpath, "*.*", SearchOption.TopDirectoryOnly);

			string prefix = DateTime.Today.ToString("yyyy-MM-dd");

			string[] logFiles = (from f in allfiles
								 where f.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
											|| f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
								 select f).ToArray();


			foreach( string file in logFiles ) {
				string key = Path.GetFileName(file);
				if( key.StartsWith(prefix) == false )		// 只显示当天的日志
					continue;

				string url = GetLogFileUrl(key);
				dict[key] = url;
			}

			return PageResult("/Log/List.cshtml", dict);
		}


		internal static string GetLogFileUrl(string logFilename)
		{
			return "/client-log.phtml?file=" 
					+ System.Web.HttpUtility.UrlEncode(CompressHelper.GzipCompress(logFilename));
		}

		private IActionResult ShowFileText(string file)
		{
            string fileName = null;
            try {
                fileName = CompressHelper.GzipDecompress(file);
            }
            catch { /* 防止人为修改URL，造成解压缩失败  */ }

            if( string.IsNullOrEmpty(fileName))
                return new TextResult("parameter error.");


            string logpath = ClientLogController.GetClientLogPath();
			string text = null;

			string filePath = Path.Combine(logpath, fileName);
			if( File.Exists(filePath)) {
				text = File.ReadAllText(filePath, Encoding.UTF8);
			}
			else {
				text = "parameter error.";
			}

			return new TextResult(text);
		}
	}
}
