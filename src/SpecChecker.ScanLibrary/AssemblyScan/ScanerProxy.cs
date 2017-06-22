using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Base;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.CoreLibrary.Common;
using SpecChecker.CoreLibrary.Models;
using SpecChecker.ScanLibrary.AssemblyScan.Checkers;

namespace SpecChecker.ScanLibrary.AssemblyScan
{
	public class ScanerProxy : MarshalByRefObject
	{
		// 定义三个检查的实例集合
		private List<IAssemblyCheck> _asmCheckers = null;
		private List<ITypeCheck> _typeCheckers = null;
		private List<IMethodCheck> _methodCheckers = null;

		private List<AssemblyScanResult> _result = new List<AssemblyScanResult>();


		private List<Type> _customizeCheckerList = new List<Type>();
		private List<KeyValuePair<string, Exception>> _invaidAssemblyList = new List<KeyValuePair<string, Exception>>();


		/// <summary>
		/// 扫描DLL是否符合规范
		/// </summary>
		/// <param name="branch"></param>
		/// <param name="option">解决方案文件名和BIN目录名</param>
		/// <returns></returns>
		public List<AssemblyScanResult> Execute(BranchSettings branch, AssemblyScanOption option)
		{
			List<Assembly> assemblies = LoadAssemblies(option);
			if( assemblies.Count == 0 )
				throw new InvalidOperationException("编译出错，没有产生程序集！");

			// 加载自定义的规则程序集
			LoadCustomizeCheckerAssembly(option.Bin);
			// 加载工具内置的检查规则
			_asmCheckers = GetCheckers<IAssemblyCheck>();
			_typeCheckers = GetCheckers<ITypeCheck>();
			_methodCheckers = GetCheckers<IMethodCheck>();

			// 扫描所有指定的程序集
			List<AssemblyScanResult> list = Scan(assemblies);

			// 过滤一些可忽略的规则
			if( string.IsNullOrEmpty(branch.IgnoreRules) == false ) {
				// 存在排除规则
				string[] rules = branch.IgnoreRules.Split(new char[] { ';' },  StringSplitOptions.RemoveEmptyEntries);
				list = (from x in list
						where rules.FirstOrDefault(r => r == x.RuleCode) == null
						select x
							).ToList();
			}



			return (from x in list
					orderby x.BusinessUnit
					select x).ToList();
		}

		private List<Assembly> LoadAssemblies(AssemblyScanOption option)
		{
			List<string> asmNames = SlnFileHelper.GetAssemblyNames(option.Sln);
			string[] files = Directory.GetFiles(option.Bin, "*.dll", SearchOption.TopDirectoryOnly);

			List<Assembly> list = new List<Assembly>();

			foreach( string file in files ) {
				string filename = Path.GetFileNameWithoutExtension(file);

				if( asmNames.FindIndex(x => x.EqualsIgnoreCase(filename)) < 0 )
					continue;

				// 过滤程序集，加快速度
				//if( filename.StartsWith("MySoft", StringComparison.OrdinalIgnoreCase) == false )
				//	continue;

				// 忽略网站项目
				if( filename.IndexOf("WebApplication", StringComparison.OrdinalIgnoreCase) > 0 )
					continue;
				if( filename.IndexOf("WebSite", StringComparison.OrdinalIgnoreCase) > 0 )
					continue;

				// 忽略测试项目的程序集
				if( filename.IndexOf("Test",  StringComparison.OrdinalIgnoreCase) > 0 )
					continue;

				try {
					Assembly assembly = Assembly.LoadFrom(file);

					if( assembly != null ) 
						list.Add(assembly);
				}
				catch( Exception ex ) {
					_invaidAssemblyList.Add(new KeyValuePair<string, Exception>(file, ex));
				}
			}
			return list;
		}


		private void LoadCustomizeCheckerAssembly(string binPath)
		{
			// 查看DLL目录中的特殊文件，它将会指出有哪些自定义的规则程序集名称
			string flagFile = Path.Combine(binPath, "CustomizeCheckerAssemblyNames.txt");
			if( File.Exists(flagFile) == false )
				return;

			string[] lines = File.ReadAllLines(flagFile, Encoding.UTF8);

			foreach(string line in lines ) {
				if( string.IsNullOrEmpty(line) )
					continue;

				string fullPath = Path.Combine(binPath, line);

				try {
					Assembly assembly = Assembly.LoadFrom(fullPath);

					if( assembly != null ) {
						// 尝试加载自定义的扫描类型
						Type t = assembly.GetType("CodeSpecification.CustomizeChecker", false, false);
						if( t != null )
							_customizeCheckerList.Add(t);
					}
				}
				catch( Exception ex ) {
					_invaidAssemblyList.Add(new KeyValuePair<string, Exception>(fullPath, ex));
				}
			}
		}


		private List<T> GetCheckers<T>()
		{
			// 从当前程序集中查找实现某个接口的类型。
			List<T> list = (from t in typeof(AssemblyScaner).Assembly.GetTypes()
							where t.IsClass && typeof(T).IsAssignableFrom(t)
							select (T)Activator.CreateInstance(t)).ToList();

			// 加载自定义的检查实现
			if( _customizeCheckerList.Count > 0 ) {
				if( typeof(T) == typeof(ITypeCheck) ) {
					foreach( Type t in _customizeCheckerList ) {
						T checker = (T)(object)CustomizeTypeCheck.Create(t);
						if( checker != null )
							list.Add(checker);
					}
				}
				else if( typeof(T) == typeof(IMethodCheck) ) {
					foreach( Type t in _customizeCheckerList ) {
						T checker = (T)(object)CustomizeMethodCheck.Create(t);
						if( checker != null )
							list.Add(checker);
					}
				}
			}

			return list;
		}


		/// <summary>
		/// 以同步方式执行代码规范检查
		/// </summary>
		/// <param name="assemblies">要检查的程序集列表</param>
		/// <returns>检查结果</returns>
		public List<AssemblyScanResult> Scan(List<Assembly> assemblies)
		{
			// 遍历所有程序集
			foreach( Assembly assembly in assemblies ) 
				ScanAssembly(assembly);


			if( _invaidAssemblyList.Count > 0 ) {
				foreach( KeyValuePair<string, Exception> kvp in _invaidAssemblyList ) {
					_result.Add(new AssemblyScanResult {
						Type = typeof(string),
						DllFileName = Path.GetFileName(kvp.Key),
						Message = "SPEC:C00000; 程序集加载失败。",
						Remark = kvp.Value.Message
					});
				}
			}

			// 加工检查结果
			foreach( AssemblyScanResult info in _result ) {
				if( info.DllFileName == null )
					info.DllFileName = info.Type.Assembly.ManifestModule.Name;

                info.BusinessUnit = BusinessUnitManager.GetNameByClass(info.Type.ToString().Split(',')[0]);

				info.RuleCode = info.GetRuleCode();
                info.TypeName = info.Type.FullName;
				info.Type = null;	// 这个类型不能带到外面的AppDomain
			}
			
			return _result;
		}

		private void ScanAssembly(Assembly assembly)
		{
			//Console.WriteLine("Scan Assembly: " + assembly.FullName);

			try {
				// 排除自动生成的程序集
				if( assembly.GetCustomAttribute<CompilerGeneratedAttribute>() != null )
					return;

				// 忽略经过混淆处理的程序集。
				var attrs = assembly.GetCustomAttributes();
				if( attrs.FirstOrDefault(a => a.GetType().Name == "DotfuscatorAttribute") != null )
					return;
			}
			catch( FileNotFoundException ex ) {
				throw new InvalidOperationException(
							"反射程序集时无法加载依赖项，当前程序集名称：" + assembly.FullName, ex);
			}

			// 调用所有的程序集检查器
			_asmCheckers.ForEach(x => CheckAssembly(x, assembly, _result));

			Type[] types = null;

			try {
				types = assembly.GetTypes();
			}
			catch( ReflectionTypeLoadException xx ) {
				foreach( Exception ex in xx.LoaderExceptions )
					_result.Add(new AssemblyScanResult {
						Type = typeof(string),
						DllFileName = assembly.ManifestModule.Name,
						Message = "SPEC:C00000; 程序集加载失败。",
						Remark = ex.Message
					});

				return;
			}

			foreach( Type t in types ) {	// 检查所有类型，包括内部类型
				try {
					ScanType(t);
				}
				catch( Exception ex ) {
					_result.Add(new AssemblyScanResult {
						Type = t,
						DllFileName = assembly.ManifestModule.Name,
						Message = "SPEC:C00000; " + ex.Message,
						Remark = ex.GetBaseException().Message
					});
				}
			}
		}


		private void ScanType(Type t)
		{
			if( t.GetCustomAttribute<CompilerGeneratedAttribute>(false) != null )
				return;

			//// 上面的判断当遇到内部再有嵌套类时，嵌套类就没有那个Attribute了，怀疑是编译器遗漏了。
			//if( t.Name.StartsWith("<") && t.IsPublic == false )
			//	continue;


			//这里只考虑一层的嵌套类型（通常情况下是够用的，如果不够用，可以修改这里。）
			//if( t.DeclaringType != null) {
			//	if (t.DeclaringType.GetCustomAttribute<CompilerGeneratedAttribute>(false) != null) {
			//		return;
			//	}
			//}

			// 忽略编译器生成的嵌套类，上面的实现方式在多层级嵌套类时会判断错误。
			if( t.FullName.IndexOf("+<>", StringComparison.Ordinal) > 0 )
				return;


			if( t.FullName.StartsWith("Microsoft.Xml.Serialization.GeneratedAssembly") )
				return;

			if( t.FullName.StartsWith("<PrivateImplementationDetails>") )
				return;

			// 调用所有的类型检查器
			_typeCheckers.ForEach(x => CheckType(x, t, _result));

			//if( t.IsClass || t.IsInterface )
			if( t.IsClass )		// 只检查类的定义，忽略接口
				// 遍历类型的所有方法

				t.GetRealMethods().ForEach(m => {
					// 调用所有的方法检查器
					_methodCheckers.ForEach(x => CheckMethod(x, m, _result));
				});
		}

		


		private void CheckAssembly(IAssemblyCheck checker, Assembly asm, List<AssemblyScanResult> result)
		{
			try {
				checker.Check(asm, result);
			}
			catch( Exception ex ) {
				string message = "检查程序集时发生异常：" + asm.FullName;
				throw new AssemblyScanException(message, ex);
			}
		}

		private void CheckType(ITypeCheck checker, Type type, List<AssemblyScanResult> result){
			try {
				checker.Check(type, result);
			}
			catch( Exception ex ) {
				string message = "检查类型时发生异常：" + type.FullName;
				throw new AssemblyScanException(message, ex);
			}
		}

		private void CheckMethod(IMethodCheck checker, MethodInfo method, List<AssemblyScanResult> result)
		{
			try {
				checker.Check(method, result);
			}
			catch( Exception ex ) {
				string message = "检查方法时发生异常：" + method.DeclaringType.FullName + "." + method.Name;
				throw new AssemblyScanException(message, ex);
			}
		}
	}
}
