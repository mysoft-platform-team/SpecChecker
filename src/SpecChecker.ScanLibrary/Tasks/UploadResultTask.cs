using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Base;
using ClownFish.Base.WebClient;
using SpecChecker.CoreLibrary;
using SpecChecker.CoreLibrary.Common;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.CoreLibrary.Models;

namespace SpecChecker.ScanLibrary.Tasks
{
	public sealed class UploadResultTask : ITask
	{
        public void Execute(TaskContext context, TaskAction action)
		{
			string website = ConfigurationManager.AppSettings["ServiceWebsite"];
			if( string.IsNullOrEmpty(website) )
				throw new ConfigurationErrorsException("ServiceWebsite没有配置。");

			TotalResult totalResult = context.TotalResult;

			// 按业务单元和扫描类别分组小计
			//CalculateSubTotal(totalResult);
			context.ConsoleWrite("ExecSubTotalResult OK");
            context.ConsoleWrite("\r\n任务结束时间：" + DateTime.Now.ToTimeString());

            // 获取控件台的所有输出内容
            totalResult.ConsoleText = context.OutputText;


			// 为了防止提交的数据过大，所以采用压缩的方式提交数据（大约可压缩10倍），
			string json = totalResult.ToJson();
			string data = CompressHelper.GzipCompress(json);


			HttpOption option = new HttpOption {
				Method = "POST",
				Url = website.TrimEnd('/') + "/ajax/scan/Upload/UploadResult.ppx",
				Data = new { base64 = data, branchId = context.Branch.Id }
			};
			option.Headers.Add("authentication-key", ConfigurationManager.AppSettings["authentication-key"]);
            option.Headers.Add("app-version", SpecChecker.CoreLibrary.Config.JobManager.AppVersion);
            string responseText = option.GetResult();
            if( responseText == "200")
                context.ConsoleWrite("UploadResultTask OK");
            else {
                context.ConsoleWrite("\r\n上传结果出现异常：##################");
                context.ConsoleWrite(responseText);
            }
                
		}


	}
}
