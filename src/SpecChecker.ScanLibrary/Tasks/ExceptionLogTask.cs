using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Log.Model;
using SpecChecker.CoreLibrary;
using SpecChecker.CoreLibrary.Common;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.ScanLibrary.ErpLog;

namespace SpecChecker.ScanLibrary.Tasks
{
	public sealed class ExceptionLogTask : ITask
	{
		public void Execute(TaskContext context, TaskAction action)
		{
			if( string.IsNullOrEmpty(context.Branch.MongoLocation) ) {
				return;
			}

			TotalResult totalResult = context.TotalResult;
			try {
				ExceptionLogScaner scaner = new ExceptionLogScaner();
				List<ExceptionInfo> list = scaner.Execute(
					DateTime.Today, DateTime.Today.AddDays(1d), context.Branch.MongoLocation);

				list = FilterResult(context, list);

				totalResult.ExceptionInfos = list;
				context.ConsoleWrite("ExceptionLogTask OK");
			}
			catch( Exception ex ) {
				totalResult.ExceptionLogException = ex.ToString();
				context.ProcessException(ex);
			}
		}


		private List<ExceptionInfo> FilterResult(TaskContext context, List<ExceptionInfo> list)
		{
            // 由于日志库是共用的，所以要根据各小组所配置的子系统获取对应的业务单元后再做过滤

			string[] bizUnitNames = context.Branch.GetBizUnitNames();
			if( bizUnitNames == null )
				return list;


			return (from x in list
					where x.BusinessInfo.Key1 == BusinessUnitManager.OthersBusinessUnitName
							|| bizUnitNames.FirstOrDefault(b => x.BusinessInfo.Key1 == b) != null
					select x).ToList();
		}
	}
}
