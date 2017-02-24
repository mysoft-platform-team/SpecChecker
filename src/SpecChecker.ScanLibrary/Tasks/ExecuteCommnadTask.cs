using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.Common;
using SpecChecker.CoreLibrary.Config;

namespace SpecChecker.ScanLibrary.Tasks
{
	public sealed class ExecuteCommnadTask : ITask
	{
		public void Execute(TaskContext context, TaskAction action)
		{
			if( action.Items == null || action.Items.Length == 0 )
				return;


			StringBuilder sb = new StringBuilder();

			foreach( string line in action.Items )
				sb.AppendLine(line);

			ExecuteResult result = ExecuteBat(sb.ToString());
			context.ConsoleWrite(result.ToString());

			context.ConsoleWrite("ExecuteCommnadTask OK");
		}


		private ExecuteResult ExecuteBat(string commandText)
		{
			ExecuteResult result = new ExecuteResult();
			result.CommandLine = "cmd.exe";

			try {
				using( System.Diagnostics.Process exe = new System.Diagnostics.Process() ) {
					exe.StartInfo.UseShellExecute = false;
					exe.StartInfo.RedirectStandardInput = true;
					exe.StartInfo.RedirectStandardOutput = true;
					exe.StartInfo.RedirectStandardError = false;
					exe.StartInfo.CreateNoWindow = true;
					exe.StartInfo.FileName = "cmd.exe";
					exe.Start();

					exe.StandardInput.WriteLine(commandText);
					exe.StandardInput.WriteLine("exit");
					exe.StandardInput.Flush();
					exe.StandardInput.Close();
					exe.WaitForExit();

					result.Output = exe.StandardOutput.ReadToEnd();
					result.ExitCode = exe.ExitCode;
				}
			}
			catch( System.Exception ex ) {
				result.ExitCode = -999;
				result.Exception = ex.ToString();
			}
			return result;
		}
	}
}
