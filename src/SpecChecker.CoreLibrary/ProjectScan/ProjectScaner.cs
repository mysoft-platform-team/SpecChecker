using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClownFish.Base.Xml;
using SpecChecker.CoreLibrary.Common;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.CoreLibrary.ProjectScan.Model;


namespace SpecChecker.CoreLibrary.ProjectScan
{
	public sealed class ProjectScaner
	{
		public List<ProjectCheckResult> Execute(BranchSettings branch, string slnFilePath)
		{
			string slnPath = Path.GetDirectoryName(slnFilePath);
			List<ProjectCheckResult> result = new List<ProjectCheckResult>();

			//string[] files = Directory.GetFiles(slnPath, "*.csproj", SearchOption.AllDirectories);
			List<string> files = SlnFileHelper.GetProjectFiles(slnFilePath);

			foreach( string filePath in files ) {
				// 忽略WEB项目
				if( filePath.IndexOf(".Web",  StringComparison.OrdinalIgnoreCase) > 0 )
					continue;

				// 忽略测试项目
				if( filePath.IndexOf("Test", StringComparison.OrdinalIgnoreCase) > 0 )
					continue;
				

				Project project = SlnFileHelper.ParseCsprojFile(filePath);

				foreach( var group in project.Groups ) {
					if( string.IsNullOrEmpty(group.Condition) )
						continue;

					Match match = Regex.Match(group.Condition, @"'(Debug|Release)\|AnyCPU'", RegexOptions.IgnoreCase);
					if( match.Success == false)
						continue;

					string configuration = match.Groups[1].Value;
					
					if( string.IsNullOrEmpty(group.OutputPath)
						|| group.OutputPath.TrimEnd('\\').Equals("bin", StringComparison.OrdinalIgnoreCase) == false ) {

						result.Add(new ProjectCheckResult {
							ProjectName = filePath.Substring(slnPath.Length + 1),
							Configuration = configuration,
							Reason = "SPEC:P00001; 请将项目的【输出路径】设置为【bin】",
							BusinessUnit = BusinessUnitManager.GetNameByFilePath(filePath)
						});
					}


					if( string.IsNullOrEmpty(group.DocumentationFile) ) {
						result.Add(new ProjectCheckResult {
							ProjectName = filePath.Substring(slnPath.Length + 1),
							Configuration = configuration,
							Reason = "SPEC:P00002; 请将【XML文档文件】设置为选中状态",
							BusinessUnit = BusinessUnitManager.GetNameByFilePath(filePath)
						});
					}

					if( string.IsNullOrEmpty(group.TreatWarningsAsErrors) ) {
						result.Add(new ProjectCheckResult {
							ProjectName = filePath.Substring(slnPath.Length + 1),
							Configuration = configuration,
							Reason = "SPEC:P00003; 请将【警告视为错误】设置为【全部】",
							BusinessUnit = BusinessUnitManager.GetNameByFilePath(filePath)
						});
					}
				}
			}


			foreach( var x in result )
				x.RuleCode = x.GetRuleCode();


			return result;
		}

	}
}
