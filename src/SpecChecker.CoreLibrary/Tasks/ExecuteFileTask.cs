using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.Common;

namespace SpecChecker.CoreLibrary.Tasks
{
	public sealed class ExecuteFileTask : ITask
	{
		public void Execute(TaskContext context, TaskAction action)
		{
			if( action.Items == null || action.Items.Length == 0 )
				return;

			
			foreach( string line in action.Items ) {
				string workingDirectory = Path.GetDirectoryName(line);
				ExecuteResult result = CommandLineHelper.Execute(line, null, workingDirectory);
				context.ConsoleWrite(result.ToString());
			}

			context.ConsoleWrite("ExecuteFileTask OK");
		}
	}
}
