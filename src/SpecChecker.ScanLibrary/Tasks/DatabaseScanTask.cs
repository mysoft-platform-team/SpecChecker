using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.CoreLibrary.Common;
using SpecChecker.CoreLibrary.Models;
using SpecChecker.CoreLibrary;
using SpecChecker.ScanLibrary.DbScan;

namespace SpecChecker.ScanLibrary.Tasks
{
	public sealed class DatabaseScanTask : ITask
	{
		public void Execute(TaskContext context, TaskAction action)
		{
			if( string.IsNullOrEmpty(context.Branch.DbLocation) ) {
				return;
			}

			TotalResult totalResult = context.TotalResult;
			try {
				DatabaseScaner scaner = new DatabaseScaner();
				List<DbCheckResult> list = scaner.Execute(context.Branch);

				//list = FilterResult(context, list);

				// 数据库的扫描结果不做累加
				totalResult.DbCheckResults = list;
				context.ConsoleWrite("DatabaseScanTask OK");
			}
			catch( Exception ex ) {
				totalResult.DbCheckException = ex.ToString();
				context.ProcessException(ex);
			}
		}


		///// <summary>
		///// 过滤结果，只包含当前任务需要包含的子系统对应的记录。
		///// 因为当前扫描任务会将数据库的所有不规范问题全部扫描出来，但是最终报表只需要显示与各小组对应的部分
		///// </summary>
		///// <typeparam name="T"></typeparam>
		///// <param name="context"></param>
		///// <param name="list"></param>
		///// <returns></returns>
		//private List<T> FilterResult<T>(TaskContext context, List<T> list) where T : BaseScanResult
		//{
		//	string[] bizUnitNames = context.Branch.GetBizUnitNames();
		//	if( bizUnitNames == null )
		//		return list;


		//	return (from x in list
		//			where x.BusinessUnit == BusinessUnitManager.OthersBusinessUnitName
		//					|| bizUnitNames.FirstOrDefault(b=> x.BusinessUnit == b) != null
		//			select x).ToList();
		//}
	}
}
