using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.UnitTest;

namespace SpecChecker.CoreLibrary.Tasks
{
	public sealed class CodeCoverTask : ITask
	{
		public void Execute(TaskContext context, TaskAction action)
		{
			if( action.Items == null || action.Items.Length == 0 )
				action.Items = context.JobOption.SlnFiles;

			if( action.Items == null || action.Items.Length == 0 )
				return;


			TotalResult totalResult = context.TotalResult;

			try {
				foreach( string path in action.Items ) {
					UnitTestRunner runner = new UnitTestRunner();
					runner.ConsoleWrite = context.ConsoleWrite;
					runner.WriteTempFile = context.WriteTempFile;

					SlnUnitTestResult total = runner.Execute(context.Branch, path);
					if( total != null ) {
						totalResult.UnitTestResults.AddRange(total.UnitTestResults);
						totalResult.CodeCoverResults.AddRange(total.CodeCoverResults);
					}
				}

				// 计算总结果
				int caseCount = 0;
				int casePassed = 0;

				int totalStatements = 0;
				int coveredStatements = 0;

				foreach( var t in totalResult.UnitTestResults ) {
					caseCount += t.Total;
					casePassed += t.Passed;
				}

				foreach( var c in totalResult.CodeCoverResults ) {
					totalStatements += c.Total;
					coveredStatements += c.Passed;
				}

				// 更新结果
				totalResult.UnitTestTotal = caseCount;
				totalResult.UnitTestPassed = casePassed;

				if( totalStatements == 0 )
					totalResult.CodeCover = 0;
				else
					totalResult.CodeCover = (int)((100 * coveredStatements) / totalStatements);


				context.ConsoleWrite("CodeCoverTask OK");
			}
			catch( Exception ex ) {
				totalResult.UnitTestPassed = -1;
				totalResult.UnitTestTotal = -1;
				context.ProcessException(ex);
			}
		}
	}
}
