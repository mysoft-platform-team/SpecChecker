using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClownFish.Base.Xml;
using SpecChecker.CoreLibrary.Models;

namespace SpecChecker.CoreLibrary.Common
{
	public static class SlnFileHelper
	{
		/// <summary>
		/// 解析解决方案文件中项目的正则表达式，
		/// 可解析的样例：Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Mysoft.Slxt.SaleService.Model", "07 售后服务\Mysoft.Slxt.SaleService.Model\Mysoft.Slxt.SaleService.Model.csproj", "{FB2475ED-FB28-4FED-8604-A3914ABF97D3}"
		/// </summary>
		private static readonly Regex s_project = new Regex(@"^Project\(""\{(?<type>[\w\-]{36})\}""\)\s+=\s+""(?<name>[^""]+)""\s*,\s+""(?<file>[^""]+)""\s*,\s+""\{(?<guid>[^""]+)\}""");

		///// <summary>
		///// 解析项目文件中的输出程序集名称
		///// </summary>
		//private static readonly Regex s_asmemblyName = new Regex(@"<AssemblyName>(?<name>[^<]+)</AssemblyName>", RegexOptions.IgnoreCase);

		///// <summary>
		///// 解析项目当前的编译模式，是Release，还是 Debug
		///// </summary>
		//private static readonly Regex s_configuration = new Regex(@"<Configuration[^>]+>(?<value>[^<]+)</Configuration>", RegexOptions.IgnoreCase);


		public static Project ParseCsprojFile(string csprojFile)
		{
			string text = File.ReadAllText(csprojFile, Encoding.UTF8);
			text = text.Replace("xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"", "xmlns=\"\"");
			var csproj = XmlHelper.XmlDeserialize<Project>(text);

			if( csproj.Groups == null || csproj.Groups.Length == 0 )
				throw new ArgumentException("无效的项目文件结构：" + csprojFile);

			return csproj;
		}

		public static List<SlnProjectInfo> GetProjects(string slnFilePath)
		{
			if( File.Exists(slnFilePath) == false )
				throw new FileNotFoundException("文件不存在：" + slnFilePath);

			if( slnFilePath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) == false )
				throw new ArgumentException("参数不是解决方案文件名。");

			List<SlnProjectInfo> list = new List<SlnProjectInfo>();

			string slnDirectory = Path.GetDirectoryName(slnFilePath);
			string[] lines = File.ReadAllLines(slnFilePath, Encoding.UTF8);

			foreach(string line in lines ) {
				Match m = s_project.Match(line);
				if( m.Success ) {
					string typeGuid = m.Groups["type"].Value;
					if( typeGuid == "2150E333-8FDC-42A3-9474-1A3956D46DE8" )        // Solution Folder
						continue;

					// http://www.cnblogs.com/landywzx/archive/2013/02/05/2893332.html
					if( typeGuid == "E24C65DC-7377-472B-9ABA-BC803B73C61A" )        // website
						continue;

					SlnProjectInfo project = new SlnProjectInfo();
					project.Name = m.Groups["name"].Value;
					project.File = m.Groups["file"].Value;
					project.Guid = m.Groups["guid"].Value;

					// 计算项目文件的全路径
					project.FullPath = Path.Combine(slnDirectory, project.File);


					// 项目的程序集名称和编译模式都是在第一个节点中指定的。
					Project csproj = ParseCsprojFile(project.FullPath);
					project.AssemblyName = csproj.Groups[0].AssemblyName;
					project.CompilerMode = csproj.Groups[0].Configuration;

					string outputPath = GetProjectOutputPath(csproj);
					if( string.IsNullOrEmpty(outputPath) )
						throw new FormatException("不能解析项目文件的编译输出目录：" + project.FullPath);
					project.OutputPath = Path.Combine(Path.GetDirectoryName(project.FullPath), outputPath);

					list.Add(project);
				}
			}
			return list;
		}

		private static string GetProjectOutputPath(Project csproj)
		{
			string compilerMode = csproj.Groups[0].Configuration;

			for(int i=1;i<csproj.Groups.Length; i++ ) {
				if( csproj.Groups[i].Condition.IndexOf(compilerMode) > 0) {
					return csproj.Groups[i].OutputPath;
				}
			}

			return null;
		}


		public static List<string> GetAssemblyNames(string slnFilePath)
		{
			List<SlnProjectInfo> projects = GetProjects(slnFilePath);

			return (from p in projects
					let n = p.AssemblyName
					where string.IsNullOrEmpty(n) == false
							&& p.IsWebApplication() == false
					select n).ToList();
		}

		private static bool IsWebApplication(this SlnProjectInfo p)
		{
			string projectPath = Path.GetDirectoryName(p.FullPath);

			// 如果在项目文件目录下发现web.config就认为是WEB项目
			string flagFile = Path.Combine(projectPath, "web.config");
			return File.Exists(flagFile);
		}


		public static List<string> GetProjectFiles(string slnFilePath)
		{
			List<SlnProjectInfo> projects = GetProjects(slnFilePath);

			return (from p in projects
					select p.FullPath).ToList();
		}


	}
}
