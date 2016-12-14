using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.ProjectScan;

namespace SpecChecker.CoreLibrary.Tasks
{
	public sealed class ProjectScanTask : ITask
	{
		public void Execute(TaskContext context, TaskAction action)
		{
			if( action.Items == null || action.Items.Length == 0 )
				action.Items = context.JobOption.SlnFiles;

			if( action.Items == null || action.Items.Length == 0 )
				return;

			TotalResult totalResult = context.TotalResult;
			List<ProjectCheckResult> resultList = new List<ProjectCheckResult>();

			try {
				foreach( string path in action.Items ) {
					ProjectScaner scaner = new ProjectScaner();
					List<ProjectCheckResult> list = scaner.Execute(context.Branch, path);
					resultList.AddRange(list);
				}

				totalResult.ProjectCheckResults.AddRange(resultList);
				context.ConsoleWrite("ProjectScanTask OK");
			}
			catch( Exception ex ) {
				totalResult.ProjectCheckException = ex.ToString();
				context.ProcessException(ex);
			}
		}
	}
}
