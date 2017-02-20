using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.CodeScan;

namespace SpecChecker.CoreLibrary.Tasks
{
	public sealed class CodeScanTask : ITask
	{
		public void Execute(TaskContext context, TaskAction action)
		{
			if( action.Items == null || action.Items.Length == 0 )
				action.Items = context.JobOption.CodePath;

			if( action.Items == null || action.Items.Length == 0 )
				return;

			TotalResult totalResult = context.TotalResult;
			List<CodeCheckResult> resultList = new List<CodeCheckResult>();

			try {
				foreach( string path in action.Items ) {
					CodeScaner scaner = new CodeScaner();
					scaner.ExcludePaths = LoadExcludePaths(path);
					List<CodeCheckResult> list = scaner.Execute(context.Branch, path);
					resultList.AddRange(list);
				}


				// 过滤前端代码的检查结果
				totalResult.JsCodeCheckResults.AddRange(
					(from x in resultList
					 where x.FileName.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
					 select x).ToList());

				// 过滤服务端代码的检查结果
				totalResult.CsCodeCheckResults.AddRange(
					(from x in resultList
					 where x.FileName.EndsWith(".js", StringComparison.OrdinalIgnoreCase) == false
					 select x).ToList());

				context.ConsoleWrite("CodeScanTask OK");
			}
			catch( Exception ex ) {
				totalResult.CodeCheckException = ex.ToString();
				context.ProcessException(ex);
			}
		}


		private string[] LoadExcludePaths(string scrPath)
		{
			string excludeSettingFile = Path.Combine(scrPath, "SpecChecker-CodeScan-Exclude-Settings.txt");
			if( File.Exists(excludeSettingFile) == false )
				return null;

			return (from x in File.ReadAllLines(excludeSettingFile, Encoding.UTF8)
					let line = x.Trim()
					where line.Length > 0
					select AddBackslash(Path.Combine(scrPath, line))
					).ToArray();
		}

		private string AddBackslash(string path)
		{
			if( path.EndsWith("\\") )
				return path;
			else
				return path + "\\";
		}
	}
}
