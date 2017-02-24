using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Base.WebClient;
using SpecChecker.CoreLibrary.Common;

namespace SpecChecker.ScanLibrary.Tasks
{
	public sealed class UploadClientLogTask : ITask
	{
		public void Execute(TaskContext context, TaskAction action)
		{
			string[] files = Directory.GetFiles(context.TempPath, "*.*", SearchOption.TopDirectoryOnly);
			if( files.Length == 0 )
				return;


			// 获取所有日志文件，仅包含：xml, txt
			string[] logFiles = (from f in files
								 where f.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
										|| f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
								 select f).ToArray();

			string zipFile = context.Branch.Id.ToString() + "__log.zip";
			string zipPath = Path.Combine(context.TempPath, zipFile);

			// 创建压缩包文件
			ZipHelper.CreateZipFile(zipPath, logFiles);

			// 上传日志
			string website = ConfigurationManager.AppSettings["ServiceWebsite"];
			HttpOption option = new HttpOption {
				Method = "POST",
				Url = website.TrimEnd('/') + "/ajax/scan/Upload/UploadClientLog.ppx",
				Data = new { logFile = new FileInfo(zipPath), flag = context.Branch.Id }
			};
			option.GetResult();

			context.ConsoleWrite("UploadClientLogTask OK");
		}
	}
}
