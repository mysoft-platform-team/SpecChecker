using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.Models;

namespace SpecChecker.ScanLibrary.AssemblyScan.Checkers
{
	internal interface IMethodCheck
	{
		void Check(MethodInfo method, List<AssemblyScanResult> result);
	}


	internal class CustomizeMethodCheck : IMethodCheck
	{
		private MethodInfo _method;
		private object _instance;
		public static CustomizeMethodCheck Create(Type t)
		{
			MethodInfo method = t.GetMethod("CheckMethod", 
									BindingFlags.Instance | BindingFlags.Public, null, 
									new Type[] { typeof(MethodInfo) }, null);

			if( method == null )
				return null;

			ConstructorInfo ctor = t.GetConstructor(Type.EmptyTypes);
			if( ctor == null )
				return null;

			CustomizeMethodCheck checker = new CustomizeMethodCheck();
			checker._method = method;
			checker._instance = Activator.CreateInstance(method.DeclaringType);
			return checker;
		}

		public void Check(MethodInfo method, List<AssemblyScanResult> result)
		{
			if( _instance == null )
				return;

			// 调用自定义的检查逻辑
			string message = (string)_method.Invoke(_instance, new object[] { method });

			if( message != null ) {
				result.Add(new AssemblyScanResult {
					Type = method.DeclaringType,
					Remark = method.GetMethodShowInfo(),
					Message = message
				});
			}
		}

	}
}
