using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClownFish.Log.Model;
using SpecChecker.CoreLibrary.CodeScan;
using SpecChecker.CoreLibrary.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SpecChecker.CoreLibrary.MethodScan
{
	public sealed class MethodScaner
	{
		public List<CodeCheckResult> Execute(string filePath)
		{
			List<MethodCheckResult> list = new List<MethodCheckResult>();

			try {
				List<MethodCodeInfo> methods = ParseMethods(filePath);
				list.AddRange(CheckMethod(methods));
			}
			catch( Exception ex ) {
				ExceptionInfo exceptionInfo = ExceptionInfo.Create(ex);
				ClownFish.Log.LogHelper.SyncWrite(exceptionInfo);

				// 这里吃掉异常是因为：没有必要因为一个文件扫描失败而结束整个扫描过程。
			}


			List<CodeCheckResult> results = new List<CodeCheckResult>();

			foreach( var mc in list ) {
				results.Add(new CodeCheckResult { 
					FileName = mc.FilePath,
					LineNo = mc.Method.LineNo,
					LineText = mc.Method.MethodName,
					Reason = mc.Reason,
					BusinessUnit = BusinessUnitManager.GetNameByFilePath(mc.FilePath)
				});
			}

			return results;
		}




		/// <summary>
		/// 解析一个C#文件中的方法定义
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		private List<MethodCodeInfo> ParseMethods(string file)
		{
			if( File.Exists(file) == false )
				throw new FileNotFoundException("文件不存在：" + file);

					
			string text = File.ReadAllText(file);
			SyntaxTree tree = CSharpSyntaxTree.ParseText(text);
			var root = (CompilationUnitSyntax)tree.GetRoot();

			List<MethodCodeInfo> list = new List<MethodCodeInfo>();

			for( int i = 0; i < root.Members.Count; i++ ) {
				NamespaceDeclarationSyntax ns = root.Members[i] as NamespaceDeclarationSyntax;
				if( ns == null )
					break;

				for( int j = 0; j < ns.Members.Count; j++ ) {
					ClassDeclarationSyntax cls = ns.Members[j] as ClassDeclarationSyntax;
					if( cls == null )
						continue;

					for( int m = 0; m < cls.Members.Count; m++ ) {
						MethodDeclarationSyntax method = cls.Members[m] as MethodDeclarationSyntax;
						if( method == null )
							continue;

						if( method.Body == null )	// abstract
							continue;

						try {
							list.Add(new MethodCodeInfo {
								FilePath = file,
								NameSpace = ns.Name.ToString(),
								ClassName = cls.Identifier.Text,
								MethodName = method.Identifier.Text,
								Body = method.Body.ToString(),
								LineNo = method.SyntaxTree.GetMappedLineSpan(method.Span).StartLinePosition.Line
							});
						}
						catch( Exception ex ) {
							ExceptionInfo exceptionInfo = ExceptionInfo.Create(ex);
							ClownFish.Log.LogHelper.SyncWrite(exceptionInfo);

							// 这里吃掉异常是因为：没有必要因为一个文件扫描失败而结束整个扫描过程。
						}
					}
				}

			}

			return list;
		}



		private List<MethodCheckResult> CheckMethod(List<MethodCodeInfo> methods)
		{
			List<MethodCheckResult> results = new List<MethodCheckResult>();

			foreach( MethodCodeInfo method in methods ) {
				int lineCount = 0;
				int commentCount = 0;

				AnalyzeMethod(method, out lineCount,  out commentCount);

				if( lineCount > 50 ) {
					results.Add(new MethodCheckResult() {
						FilePath = method.FilePath,
						Method = method,
						Reason = "SPEC:C00027; 每个方法体不允许大于50行（有效代码）"
					});
				}

				if( lineCount > 10 ) {
					int c2 = lineCount / 3;

					if( commentCount < c2 )
						results.Add(new MethodCheckResult() {
							FilePath = method.FilePath,
							Method = method,
							Reason = "SPEC:C00028; 大于10行（有效代码）的方法，注释行/代码有效行数之比要大于1/3"
						});
				}
			}

			return results;
		}


		/// <summary>
		/// 匹配有效注释（至少包含5个连续汉字）
		/// </summary>
		private static readonly Regex s_chineseRegex = new Regex(@"//.+[\u4e00-\u9fa5]{5,}", RegexOptions.Compiled);

		private void AnalyzeMethod(MethodCodeInfo method, out int lineCount, out int commentCount)
		{
			lineCount = 0;
			commentCount = 0;

			using( StringReader reader = new StringReader(method.Body) ) {
				string line = null;
				while( (line = reader.ReadLine()) != null ) {
					line = line.Trim();

					if( line.Length < 5 )		// 忽略无效的代码，例如：花括号
						continue;

					if( line == "continue;"
						|| line == "break;"
						//|| line.EndsWith('=')	// 赋值语句折行
						//|| line.EndsWith("(")	// 方法调用拆行
						//|| line.EndsWith(',')	// 赋值或者调用方法拆行						
						//|| line.StartsWith("&&")	// 判断逻辑拆行
						//|| line.EndsWith("&&")		// 判断逻辑拆行
						//|| line.StartsWith("||")	// 判断逻辑拆行
						//|| line.EndsWith("||")		// 判断逻辑拆行
						//|| line.StartsWith("?")		// 判断逻辑拆行
						//|| line.StartsWith(":")		// 判断逻辑拆行
						//|| (line.IndexOf(" = new ") > 0 && line.IndexOf(',') < 0)	// 创建对象拆行
						)
						continue;

					if( line.StartsWith("//") ) {	// 注释行
						if( s_chineseRegex.IsMatch(line) )
							commentCount++;
					}
					else {
						if( line.StartsWith("if")
							|| line.StartsWith("for")
							|| line.StartsWith("foreach")
							|| line.StartsWith("while")
							|| line.EndsWith(";")
							)
							lineCount++;
					}

					
				}
			}
		}





	}
}
