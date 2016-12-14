using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.AssemblyScan;
using SpecChecker.CoreLibrary.Common;

namespace SpecChecker.CoreLibrary.Tasks
{
	public sealed class AssemblyScanTask : ITask
	{
		public void Execute(TaskContext context, TaskAction action)
		{
			if( action.Items == null )
				return;

			TotalResult totalResult = context.TotalResult;
			List<AssemblyScanResult> resultList = new List<AssemblyScanResult>();

			try {
				foreach( string line in action.Items ) {
					AssemblyScaner scaner = new AssemblyScaner();

					TextLineSerializer serializer = new TextLineSerializer();
					AssemblyScanOption option = serializer.Deserialize<AssemblyScanOption>(line);
					if( string.IsNullOrEmpty(option.Bin) )
						throw new ArgumentException("AssemblyScan任务的参数不正确，没有指定 bin 参数");

					if( string.IsNullOrEmpty(option.Sln) ) {
						if( context.JobOption.SlnFiles != null && context.JobOption.SlnFiles.Length == 1 )
							option.Sln = context.JobOption.SlnFiles[0];
						else
							throw new ArgumentException("AssemblyScan任务的参数不正确，没有指定 sln 参数");
					}

					List<AssemblyScanResult> list = scaner.Execute(context.Branch, option);
					resultList.AddRange(list);
				}


				totalResult.RuntimeScanResults.AddRange(resultList);

				context.ConsoleWrite("AssemblyScanTask OK");
			}
			catch( Exception ex ) {
				totalResult.RuntimeScanException = ex.ToString();
				context.ProcessException(ex);
			}
		}
	}
}
