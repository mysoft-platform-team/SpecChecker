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
	internal class MethodChecker : IMethodCheck
	{
		private MethodInfo _method;
		private ParameterInfo[] _parameters;
		private List<AssemblyScanResult> _result;


		public void Check(MethodInfo method, List<AssemblyScanResult> result)
		{
			if ( method.GetCustomAttribute<CompilerGeneratedAttribute>() != null)
				return;

			_method = method;
			_result = result;
			_parameters = method.GetParameters();


			CheckMethodName();
			CheckParameterName();
			CheckParameterCount();
			//CheckDynamicParameter();
		}


		private void CheckMethodName()
		{
			if( _method.Name[0].IsUpper() == false && _method.IsSpecialName == false ) {

				if( _parameters.Length == 2
					&& _method.IsPublic == false
					&& _parameters[0].ParameterType == typeof(object) && _parameters[0].Name == "sender"
					&& _parameters[1].ParameterType == typeof(EventArgs) && _parameters[1].Name == "e" ) {
					// 这是标准的事件订阅代码，而且还是IDE自动生成的，因此先允许这种代码。
				}
				else {
					_result.Add(new AssemblyScanResult {
						Type = _method.DeclaringType,
						Remark = _method.GetMethodShowInfo(),
						Message = "SPEC:R00015; 方法名称的首字母不是大写开头"
					});
				}
			}
		}

		private void CheckParameterName()
		{
			foreach( ParameterInfo p in _parameters )
				if( p.Name[0].IsUpper() )
					_result.Add(new AssemblyScanResult {
						Type = _method.DeclaringType,
						Remark = _method.GetMethodShowInfo(),
						Message = "SPEC:R00016; 方法的参数名称的首字母不是小写开头"
					});
		}
		private void CheckParameterCount()
		{
			// 只检查公开的方法。
			if( (_method.IsPublic || _method.IsAssembly)
				&& _method.DeclaringType.IsPublic
				&& _parameters.Length > 5 )
				// 用于优化反射的类库中，为了支持多参数方法的优化，因此某些类型的方法参数也比较多，所以只能忽略。
				if( _method.DeclaringType.FullName.IndexOf(".OptimizeReflection.") < 0 )
					_result.Add(new AssemblyScanResult {
						Type = _method.DeclaringType,
						Remark = _method.GetMethodShowInfo(),
						Message = "SPEC:R00017; 方法的参数已超过 5个"
					});
		}

		//private void CheckDynamicParameter()
		//{
		//	foreach( ParameterInfo p in _parameters )
		//		if( p.ParameterType == typeof(object)
		//			&& p.GetCustomAttribute<CompilerGeneratedAttribute>() != null
		//			)
		//			_result.Add(new AssemblyScanResult {
		//				Type = _method.DeclaringType,
		//				Remark = _method.GetMethodShowInfo(),
		//				Message = "SPEC:R00039; 方法的参数不允许使用dynamic类型"
		//			});

		//}

	}
}
