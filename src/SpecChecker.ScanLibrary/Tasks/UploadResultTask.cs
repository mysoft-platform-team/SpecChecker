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
            context.ConsoleWrite("\r\n结束时间：" + DateTime.Now.ToTimeString());

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
			option.GetResult();

			context.ConsoleWrite("UploadResultTask OK");
		}

		//private void EvalSubTotalRows<T>(
		//	List<SubTotalResult> list,                              // 用于填充返回值的列表
		//	List<T> scanResultList,                                 // 扫描数据
		//	string scanKind) where T : BaseScanResult               // 当前操作的数据类别
		//{
		//	if( scanResultList == null )
		//		return;


		//	(from x in scanResultList
		//	 group x by x.BusinessUnit into g
		//	 select new SubTotalResult { ScanKind = scanKind, BusinessUnit = g.Key, Count = g.Count() }
		//			 ).ToList().ForEach(
		//				y => list.Add(y)
		//			 );
		//}

		///// <summary>
		///// 计算分类汇总（按扫描类别和业务单元），计算的结果直接写入 totalResult.Summary属性。
		///// </summary>
		///// <param name="totalResult"></param>
		//private void CalculateSubTotal(TotalResult totalResult)
		//{
		//	List<SubTotalResult> list = new List<SubTotalResult>();

		//	EvalSubTotalRows(list, totalResult.RuntimeScanResults, "程序集扫描结果");

		//	EvalSubTotalRows(list, totalResult.DbCheckResults, "数据库扫描结果");

		//	EvalSubTotalRows(list, totalResult.JsCodeCheckResults, "前端代码扫描结果");

		//	EvalSubTotalRows(list, totalResult.CsCodeCheckResults, "后端代码扫描结果");

		//	EvalSubTotalRows(list, totalResult.ProjectCheckResults, "项目设置检查结果");

		//	EvalSubTotalRows(list, totalResult.VsRuleCheckResults, "微软规则扫描结果");


		//	if( totalResult.ExceptionInfos != null ) {
		//		(from x in totalResult.ExceptionInfos
		//		 group x by x.BusinessInfo.Key1 into g
		//		 select new SubTotalResult { ScanKind = "异常日志", BusinessUnit = g.Key, Count = g.Count() }
		//			 ).ToList().ForEach(
		//				y => list.Add(y)
		//			 );
		//	}

		//	if( totalResult.PerformanceInfos != null ) {
		//		(from x in totalResult.PerformanceInfos
		//		 group x by x.BusinessInfo.Key1 into g
		//		 select new SubTotalResult { ScanKind = "性能日志", BusinessUnit = g.Key, Count = g.Count() }
		//			 ).ToList().ForEach(
		//				y => list.Add(y)
		//			 );
		//	}

		//	totalResult.Summary = list;
		//}
	}
}
