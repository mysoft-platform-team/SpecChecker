using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SpecChecker.CoreLibrary.AssemblyScan.Checkers
{
	internal class MethodChecker : IMethodCheck
	{

		public void Check(MethodInfo method, List<AssemblyScanResult> result)
		{
			if (method.GetCustomAttribute<CompilerGeneratedAttribute>() != null)
				return;

			ParameterInfo[] parameters = method.GetParameters();

			if (method.Name[0].IsUpper() == false && method.IsSpecialName == false) {

				if (parameters.Length == 2
				    && method.IsPublic == false
					&& parameters[0].ParameterType == typeof(object) && parameters[0].Name == "sender"
					&& parameters[1].ParameterType == typeof(EventArgs) && parameters[1].Name == "e" ) {
					// 这是标准的事件订阅代码，而且还是IDE自动生成的，因此先允许这种代码。
				}
				else {
					result.Add(new AssemblyScanResult{
						Type = method.DeclaringType,
						Remark = method.GetMethodShowInfo(),
						Message = "SPEC:R00015; 方法名称的首字母不是大写开头"
					});
				}
			}


			foreach (ParameterInfo p in parameters)
				if (p.Name[0].IsUpper()) 
					result.Add(new AssemblyScanResult{
						Type = method.DeclaringType,
						Remark = method.GetMethodShowInfo(),
						Message = "SPEC:R00016; 方法的参数名称的首字母不是小写开头"
					});
	

			// 只检查公开的方法。
			if ((method.IsPublic || method.IsAssembly) 
				&& method.DeclaringType.IsPublic 
				&& parameters.Length > 5)
				// 用于优化反射的类库中，为了支持多参数方法的优化，因此某些类型的方法参数也比较多，所以只能忽略。
				if (method.DeclaringType.FullName.IndexOf(".OptimizeReflection.") < 0)
					result.Add(new AssemblyScanResult{
						Type = method.DeclaringType,
						Remark = method.GetMethodShowInfo(),
						Message = "SPEC:R00017; 方法的参数已超过 5个"
					});

		}

	}
}
