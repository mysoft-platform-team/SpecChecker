using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Web;

namespace SpecChecker.CoreLibrary.AssemblyScan.Checkers
{
	internal class ServiceMethodChecker : IMethodCheck
	{

		public void Check(MethodInfo method, List<AssemblyScanResult> result)
		{
			if( method.GetCustomAttribute<CompilerGeneratedAttribute>() != null )
				return;

			// 只检查公开方法
			if( method.IsPublic == false )
				return;

			// 只检查服务方法
			if( method.DeclaringType.Name.EndsWith("Service", StringComparison.OrdinalIgnoreCase) == false )
				return;


			if ((method.IsPublic || method.IsAssembly)
					&& (method.IsStatic == false)
					&& (method.IsVirtual == false || method.IsFinal))

				result.Add(new AssemblyScanResult {
					Type = method.DeclaringType,
					Remark = method.GetMethodShowInfo(),
					Message = "SPEC:R00025; 服务类型的公开实例方法需要定义成虚方法" // 静态方法也不能使用。
				});



			// ERP特定的检查范围
			if (method.DeclaringType.Name.EndsWith("AppService", StringComparison.OrdinalIgnoreCase)) {

				// 判断方法是否禁止HTTP调用， 由于ForbidHttpAttribute定义在ERP中，所以用名字来判断
				object[] attrs = method.GetCustomAttributes(true);
				foreach(object attr in attrs ) {
					if( attr.GetType().Name.EndsWith("ForbidHttpAttribute") )
						return;
				}

				// 如果不是MVC要求的ActionResult，就要求数据类型是可序列化的
				if ( typeof(IActionResult).IsAssignableFrom(method.ReturnType) == false 
					&& method.ReturnType.IsSerializable() == false
					) {
					result.Add(new AssemblyScanResult {
						Type = method.DeclaringType,
						Remark = method.GetMethodShowInfo(),
						Message = "SPEC:R00024; 服务方法的返回值类型不是可序列化的"
					});
				}

				ParameterInfo[] parameters = method.GetParameters();
				foreach (ParameterInfo p in parameters) {
					if (p.ParameterType.IsSerializable() == false) {
						result.Add(new AssemblyScanResult {
							Type = method.DeclaringType,
							Remark = method.GetMethodShowInfo(),
							Message = "SPEC:R00026; 服务方法的参数类型不是可序列化的"
						});
						break;
					}
					
					if (p.ParameterType.IsByRef) {
						result.Add(new AssemblyScanResult {
							Type = method.DeclaringType,
							Remark = method.GetMethodShowInfo(),
							Message = "SPEC:R00027; 服务方法的参数类型不允许用 ref, out 修饰"
						});
						break;
					}
				}
			}

		}

	}
}
