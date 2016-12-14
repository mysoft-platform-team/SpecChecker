using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Log.Model;

namespace SpecChecker.CoreLibrary.Common
{
	public static class CommandLineHelper
	{
		public static ExecuteResult Execute(string exePath, string argument = null, string workingDirectory = null)
		{
			ExecuteResult result = new ExecuteResult();
			result.CommandLine = exePath + " " + argument;

			try {
				using( System.Diagnostics.Process exe = new System.Diagnostics.Process() ) {
					exe.StartInfo.UseShellExecute = false;
					exe.StartInfo.RedirectStandardOutput = true;
					exe.StartInfo.RedirectStandardError = false;
					exe.StartInfo.CreateNoWindow = true;
					exe.StartInfo.FileName = exePath;
					if( string.IsNullOrEmpty(argument) == false)
						exe.StartInfo.Arguments = argument;
					if( string.IsNullOrEmpty(workingDirectory) == false )
						exe.StartInfo.WorkingDirectory = workingDirectory;
					exe.Start();

					string text;
					StringBuilder sb = new StringBuilder();
					while( (text = exe.StandardOutput.ReadLine()) != null ) {
						text = text.TrimEnd(new char[0]);
						if( text.Length > 0 ) {
							sb.AppendLine(text);
						}
					}
					result.Output = sb.ToString();

					//exe.WaitForExit();		// 跑单元测试时，有时候会挂死！
					//result.Output = exe.StandardOutput.ReadToEnd();
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


	public sealed class ExecuteResult : BaseInfo
	{
		public string CommandLine { get; set; }

		public string Output { get; set; }

		public string Exception { get; set; }

		public int ExitCode { get; set; }

		public override string ToString()
		{
			return string.Format(@"
================================================================================
{0}
-----------------------------------------------------------------------------
{1}
-----------------------------------------------------------------------------
",
				this.CommandLine,
				(this.Exception == null ? this.Output : this.Exception.ToString()));
		}
	}
}
