using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Base;
using ClownFish.Base.Http;
using ClownFish.Web;
using SpecChecker.CoreLibrary;
using SpecChecker.CoreLibrary.Common;
using SpecChecker.WebLib.Common;
using SpecChecker.WebLib.Controllers;

namespace SpecChecker.WebLib.Services
{
	public class UploadService : MyBaseController
	{
		public string UploadResult(string base64, int branchId)
		{
			string authkey = ConfigurationManager.AppSettings["authentication-key"];
			if( this.GetHeader("authentication-key") != authkey )
				return "authentication-key is invalid.";


			// 为了防止提交的数据过大，所以采用压缩的方式提交数据（大约可压缩10倍），
			// 为了方便调试，将压缩后的数据以BASE64方式传输
			string json = CompressHelper.GzipDecompress(base64);
			TotalResult result = json.FromJson<TotalResult>();

			// 设置问题分类
			result.SetIssueCategory();
			json = result.ToJson();		// 上面的调用会修改数据，所以重新生成JSON

			DateTime today = DateTime.Today;

			string filename = ScanResultCache.GetTotalResultFilePath(branchId, today);
			string uploadPath = Path.GetDirectoryName(filename);

			if( Directory.Exists(uploadPath) == false )
				Directory.CreateDirectory(uploadPath);

			// JSON文本的体积小，序列化/反序列化更快，而且特殊字符的支持更好，所以这里使用JSON，不再使用XML
			//XmlHelper.XmlSerializeToFile(result, filename, Encoding.UTF8);

			//File.WriteAllText(filename, json, Encoding.UTF8);
			ZipHelper.CreateZipFileFromText(filename, json);
			

			// 刷新小组汇总数据
			// 这个任务只能放在服务端完成，因为客户端没有完整的数据
			DailySummaryHelper helper = new DailySummaryHelper();
			helper.RefreshDailySummary(today);


			// 清除缓存
			ScanResultCache.RemoveCache(branchId, today);


			// 发送通知邮件
			Uri requestUri = this.HttpContext.Request.Url;
			SendEmailService.Send(today, branchId, requestUri);

			return "OK";
		}



		public string UploadClientLog(HttpFile logFile, int flag)
		{
			string savepath = ClientLogController.GetClientLogPath();

			string filename = string.Format("{0}_{1}.zip", flag, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
			string zipPath = Path.Combine(savepath, filename);

			if( File.Exists(zipPath) )
				File.Delete(zipPath);

			// 将上传文件先保存到临时目录
			File.WriteAllBytes(zipPath, logFile.FileBody);

			// 释放压缩包中的日志文件，如果存在就覆盖
			ZipHelper.ExtractFiles(zipPath, savepath);

			System.Threading.Thread.Sleep(2000);
			return "OK";
		}

	}
}
