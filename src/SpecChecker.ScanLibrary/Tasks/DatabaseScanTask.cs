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
			if( string.IsNullOrEmpty(context.Branch.DbLocation) ) 
				return;
			

			TotalResult totalResult = context.TotalResult;
			try {
				DatabaseScaner scaner = new DatabaseScaner();
				List<DbCheckResult> list = scaner.Execute(context.Branch);


				// 数据库的扫描结果不做累加
				totalResult.DbCheckResults = list;
				context.ConsoleWrite("DatabaseScanTask OK");
			}
			catch( Exception ex ) {
				totalResult.DbCheckException = ex.ToString();
				context.ProcessException(ex);
			}
		}


	}
}
