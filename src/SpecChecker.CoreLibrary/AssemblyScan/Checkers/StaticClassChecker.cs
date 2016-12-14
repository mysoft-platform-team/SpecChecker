using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SpecChecker.CoreLibrary.AssemblyScan.Checkers
{
	internal class StaticClassChecker : ITypeCheck 
	{
		public void Check(Type type, List<AssemblyScanResult> result)
		{
			// 不检查接口

			if( type.IsClass ) {

				if( type.IsSealed && type.IsAbstract ) {
					// 静态类

					ExtensionAttribute a = type.GetCustomAttribute<ExtensionAttribute>(false);
					if( a != null 
						&& type.Name.EndsWith("Extensions", StringComparison.Ordinal) == false
						&& type.Name.EndsWith("Helper", StringComparison.Ordinal) == false )
						result.Add(new AssemblyScanResult {
							Type = type,
							Message = "SPEC:R00028; 扩展类型必须以 Extensions 结尾"
						});
				}
				else {
					// 非静态类

					MemberInfo[] m1 = type.GetMembers(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

					MemberInfo[] m2 = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

					if( m2.Length == 0 && m1.Length > 0 ) {
						result.Add(new AssemblyScanResult {
							Type = type,
							Message = "SPEC:R00029; 类型的所有成员都是静态的，类型必须申明成静态的"
						});
					}
				}
			}

			
		}

	}
}
