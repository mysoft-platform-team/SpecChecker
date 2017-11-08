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

                DateTime start, end;
                if( DateTime.Now.Hour < 12 ) {
                    // 有些产品组只在凌晨运行扫描工具，会导致当天的数据几乎是零，
                    // 所以，这里的规则是：如果在 12点前运行工具，就取前一天的数据
                    start = DateTime.Today.AddDays(-1);
                    end = DateTime.Today;
                }
                else {
                    start = DateTime.Today;
                    end = DateTime.Today.AddDays(1d);
                }

                List<PerformanceInfo> list = isMongoDb
                                        ? scaner.Execute(start, end, context.Branch.MongoLocation)
                                        : scaner.Execute2(start, end, context.Branch.MongoLocation);


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
