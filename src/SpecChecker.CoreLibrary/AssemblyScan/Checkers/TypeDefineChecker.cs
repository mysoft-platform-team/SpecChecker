using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.AssemblyScan.Checkers
{
	internal class TypeDefineChecker : ITypeCheck
	{
		public void Check(Type type, List<AssemblyScanResult> result)
		{
			if( type.IsValueType && type.IsEnum == false )
				result.Add(new AssemblyScanResult {
					Type = type,
					Message = "SPEC:R00030; 不允许定义结构体类型"
				});


			if( type.IsClass ) {
				MethodInfo m = type.GetMethod("Finalize", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
				if( m != null && m.ReturnType == typeof(void) )
					result.Add(new AssemblyScanResult {
						Type = type,
						Message = "SPEC:R00031; 类型不允许定义析构函数"
					});


				if( type.IsNested && type.IsPublic )
					result.Add(new AssemblyScanResult {
						Type = type,
						Message = "SPEC:R00032; 嵌套类型不允许定义成 Public"
					});


				MemberInfo[] members = type.GetMembers(SomeExtensions.DefaultBindingFlags);

				if( members.Length == 0
					// 排除构造函数，所以不是和零比较。
					|| (members.Length == 1 && members[0].MemberType == MemberTypes.Constructor)
					)

					// 需要排除 Attribute 或者实现接口的类型，因为这二种东西有时候就是用于标记用的。
					// 例如：MyMVC.ActionAssemblyAttribute
					//			IRequiresSessionState, IReadOnlySessionState

					//if( type.IsSubclassOf(typeof(Attribute)) == false 
					if( type.BaseType == typeof(System.Object) 	// 允许派生类做为标记类使用，与空接口的效果类似
						&& type.GetInterfaces().Length == 0)
						result.Add(new AssemblyScanResult {
							Type = type,
							Message = "SPEC:R00033; 不允许定义空类型（不包含任何成员）"		// 空接口暂且允许。
						});
			}

		}

	}
}
