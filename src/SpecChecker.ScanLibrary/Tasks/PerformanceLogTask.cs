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
	public sealed class PerformanceLogTask : ITask
	{
		public void Execute(TaskContext context, TaskAction action)
		{
			if( string.IsNullOrEmpty(context.Branch.MongoLocation) ) 
				return;
			

			TotalResult totalResult = context.TotalResult;
			try {
				PerformanceLogScaner scaner = new PerformanceLogScaner();

				bool isMongoDb = context.Branch.MongoLocation.StartsWith("mongodb", StringComparison.OrdinalIgnoreCase);

				List<PerformanceInfo> list = isMongoDb
				? scaner.Execute(DateTime.Today, DateTime.Today.AddDays(1d), context.Branch.MongoLocation)
				: scaner.Execute2(DateTime.Today, DateTime.Today.AddDays(1d), context.Branch.MongoLocation);


				totalResult.PerformanceInfos = list;
				context.ConsoleWrite("PerformanceLogTask OK");
			}
			catch( Exception ex ) {
				totalResult.PerformanceLogException = ex.ToString();
				context.ProcessException(ex);
			}
		}

	}
}
