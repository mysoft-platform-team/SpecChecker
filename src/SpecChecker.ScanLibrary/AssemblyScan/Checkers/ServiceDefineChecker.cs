using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;
using SpecChecker.CoreLibrary.Models;

namespace SpecChecker.ScanLibrary.AssemblyScan.Checkers
{
	internal class ServiceDefineChecker : ITypeCheck
	{
		public void Check(Type type, List<AssemblyScanResult> result)
		{
			if( type.IsClass && type.Name.EndsWith("Service", StringComparison.OrdinalIgnoreCase) ) {

				if( type.IsSealed && type.IsAbstract )
					result.Add(new AssemblyScanResult {
						Type = type,
						Message = "SPEC:R00020; 服务类型不允许定义成静态类"
					});


				if( type.IsSealed )
					result.Add(new AssemblyScanResult {
						Type = type,
						Message = "SPEC:R00021; 服务类型不能定义成 封闭类"
					});


				// 下面这个写法还有点小缺陷：把静态事件也包含进去了。
				FieldInfo field = (from f in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
								   where f.GetCustomAttribute<CompilerGeneratedAttribute>() == null 
										&& f.IsLiteral == false
										// 允许定义【正则表达式】或者【字符串】，因为它们是不可变的。
										&& (f.FieldType != typeof(System.Text.RegularExpressions.Regex) || f.FieldType != typeof(string))
								   select f).FirstOrDefault();
				if( field != null  )
					result.Add(new AssemblyScanResult {
						Type = type,
						Message = "SPEC:R00022; 服务类型不允许定义静态成员（字段，属性，方法）",
						Remark = "Field: " + field.ToString()
					});
				else {
					MethodInfo method = (from f in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
										 where f.GetCustomAttribute<CompilerGeneratedAttribute>() == null
										 select f).FirstOrDefault();

					if( method != null )
						result.Add(new AssemblyScanResult {
							Type = type,
							Message = "SPEC:R00023; 服务类型不允许定义静态成员（字段，属性，方法）",
							Remark = "Method: " + method.GetMethodShowInfo()
						});
				}

			
			}
		}

	}
}
