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
			if( string.IsNullOrEmpty(context.Branch.MongoLocation) ) 
				return;
			

			TotalResult totalResult = context.TotalResult;
			try {
				ExceptionLogScaner scaner = new ExceptionLogScaner();

				bool isMongoDb = context.Branch.MongoLocation.StartsWith("mongodb", StringComparison.OrdinalIgnoreCase);

				List<ExceptionInfo> list = isMongoDb
				? scaner.Execute(DateTime.Today, DateTime.Today.AddDays(1d), context.Branch.MongoLocation)
				: scaner.Execute2(DateTime.Today, DateTime.Today.AddDays(1d), context.Branch.MongoLocation);


				totalResult.ExceptionInfos = list;
				context.ConsoleWrite("ExceptionLogTask OK");
			}
			catch( Exception ex ) {
				totalResult.ExceptionLogException = ex.ToString();
				context.ProcessException(ex);
			}
		}


	}
}
