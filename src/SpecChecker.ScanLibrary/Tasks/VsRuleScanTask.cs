using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.CoreLibrary.Models;
using SpecChecker.ScanLibrary.VsRuleScan;

namespace SpecChecker.ScanLibrary.Tasks
{
	public sealed class VsRuleScanTask : ITask
	{
		public void Execute(TaskContext context, TaskAction action)
		{
			if( action.Items == null || action.Items.Length == 0 )
				action.Items = context.JobOption.SlnFiles;

			if( action.Items == null || action.Items.Length == 0 )
				return;

			TotalResult totalResult = context.TotalResult;
			List<VsRuleCheckResult> resultList = new List<VsRuleCheckResult>();

			try {
				foreach( string path in action.Items ) {
					VsRuleScaner scaner = new VsRuleScaner();
					List<VsRuleCheckResult> list = scaner.Execute(context.Branch, path);
					resultList.AddRange(list);
				}
				totalResult.VsRuleCheckResults.AddRange(resultList);
				context.ConsoleWrite("VsRuleScanTask OK");
			}
			catch( Exception ex ) {
				totalResult.VsRuleCheckException = ex.ToString();
				context.ProcessException(ex);
			}
		}
	}
}
