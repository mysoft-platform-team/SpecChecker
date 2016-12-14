using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Log.Model;
using SpecChecker.CoreLibrary.Common;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.CoreLibrary.ErpLog;

namespace SpecChecker.CoreLibrary.Tasks
{
	public sealed class PerformanceLogTask : ITask
	{
		public void Execute(TaskContext context, TaskAction action)
		{
			if( string.IsNullOrEmpty(context.Branch.MongoLocation) ) {
				return;
			}

			TotalResult totalResult = context.TotalResult;
			try {
				PerformanceLogScaner scaner = new PerformanceLogScaner();
				List<PerformanceInfo> list = scaner.Execute(
					DateTime.Today, DateTime.Today.AddDays(1d), context.Branch.MongoLocation);

				list = FilterResult(context, list);

				totalResult.PerformanceInfos = list;
				context.ConsoleWrite("PerformanceLogTask OK");
			}
			catch( Exception ex ) {
				totalResult.PerformanceLogException = ex.ToString();
				context.ProcessException(ex);
			}
		}

		private List<PerformanceInfo> FilterResult(TaskContext context, List<PerformanceInfo> list)
		{
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
