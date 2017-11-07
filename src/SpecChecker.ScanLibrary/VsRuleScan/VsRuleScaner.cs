using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Base.Xml;
using SpecChecker.CoreLibrary.Common;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.CoreLibrary.Models;
using SpecChecker.ScanLibrary.VsRuleScan.Models;

namespace SpecChecker.ScanLibrary.VsRuleScan
{
	public class VsRuleScaner
	{
		private List<string> GetCodeAnalysisLogFiles(string slnFilePath)
		{
			List<SlnProjectInfo> list = SlnFileHelper.GetProjects(slnFilePath);
			List<string> files = new List<string>();

			foreach( SlnProjectInfo project in list ) {
				if( string.IsNullOrEmpty(project.OutputPath) || string.IsNullOrEmpty(project.AssemblyName) )
					continue;

				string file = Path.Combine(project.OutputPath, project.AssemblyName + ".dll.CodeAnalysisLog.xml");
				files.Add(file);
			}

			return files;
		}
		public List<VsRuleCheckResult> Execute(BranchSettings branch, string slnFilePath)
		{
			string slnPath = Path.GetDirectoryName(slnFilePath);
			//string[] files = Directory.GetFiles(slnPath, "*.CodeAnalysisLog.xml", SearchOption.AllDirectories);
			List<string> files = GetCodeAnalysisLogFiles(slnFilePath);

			List <VsRuleCheckResult> list = new List<VsRuleCheckResult>();
			List<Rule> rules = new List<Rule>();

			foreach( string file in files ) { 
				if( File.Exists(file) == false )
					continue;

				FxCopReport report = XmlHelper.XmlDeserializeFromFile<FxCopReport>(file);

				if( report.Rules != null )
					rules.AddRange(report.Rules);

				if( report.Targets != null ) {
					foreach( Target target in report.Targets ) {
						foreach( Module module in target.Modules ) {
							foreach( Namespace ns in module.Namespaces ) {
								foreach( var t in ns.Types ) {
									if( t.Messages != null ) {
										foreach( Message message in t.Messages ) {
											foreach( Issue issue in message.Issues ) {
												//if( issue.Path != null && issue.File != null )
												list.Add(new VsRuleCheckResult {
													CheckId = message.CheckId,
													Url = rules.First(x => x.CheckId == message.CheckId).Url,
													Message = issue.Message,
													File = (issue.Path == null || issue.File == null) 
															? t.Name 
															: Path.Combine(issue.Path, issue.File).Substring(slnPath.Length + 1),
													Line = issue.Line
												});
											}
										}
									}

									foreach( Member member in t.Members ) {
										if( member.Accessors != null ) {
											foreach(Accessor a in member.Accessors)
												foreach( Message message in a.Messages ) {
													foreach( Issue issue in message.Issues ) {
														//if( issue.Path != null && issue.File != null )
														list.Add(new VsRuleCheckResult {
															CheckId = message.CheckId,
															Url = rules.First(x => x.CheckId == message.CheckId).Url,
															Message = issue.Message,
															File = (issue.Path == null || issue.File == null) 
																? a.Name 
																: Path.Combine(issue.Path, issue.File).Substring(slnPath.Length + 1),
															Line = issue.Line
														});
													}
												}
										}

										if( member.Messages != null ) {
											foreach( Message message in member.Messages ) {
												foreach( Issue issue in message.Issues ) {
													//if( issue.Path != null && issue.File != null )
													list.Add(new VsRuleCheckResult {
														CheckId = message.CheckId,
														Url = rules.First(x => x.CheckId == message.CheckId).Url,
														Message = issue.Message,
														File = (issue.Path == null || issue.File == null) 
															? (ns.Name + "." + t.Name) 
															: Path.Combine(issue.Path, issue.File).Substring(slnPath.Length + 1),
														Line = issue.Line
													});
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}

			foreach( var result in list ) {
				result.RuleCode = result.GetRuleCode();
			}

			// 排除这条规则，因为它基本上是误报！
			list = (from x in list
					where x.CheckId != "CA2202"
					select x
						).ToList();


			return list;
		}

	}
}
