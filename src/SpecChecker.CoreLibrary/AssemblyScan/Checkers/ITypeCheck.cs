using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.AssemblyScan.Checkers
{
	internal interface ITypeCheck
	{
		void Check(Type type, List<AssemblyScanResult> result);
	}


	internal class CustomizeTypeCheck : ITypeCheck
	{
		private MethodInfo _method;
		private object _instance;
		public static CustomizeTypeCheck Create(Type t)
		{
			MethodInfo method = t.GetMethod("CheckType",
									BindingFlags.Instance | BindingFlags.Public, null,
									new Type[] { typeof(Type) }, null);

			if( method == null )
				return null;

			ConstructorInfo ctor = t.GetConstructor(Type.EmptyTypes);
			if( ctor == null )
				return null;

			CustomizeTypeCheck checker = new CustomizeTypeCheck();
			checker._method = method;
			checker._instance = Activator.CreateInstance(method.DeclaringType);
			return checker;
		}

		public void Check(Type type, List<AssemblyScanResult> result)
		{
			if( _instance == null )
				return;

			// 调用自定义的检查逻辑
			string message = (string)_method.Invoke(_instance, new object[] { type });

			if( message != null ) {
				result.Add(new AssemblyScanResult {
					Type = type,
					Message = message
				});
			}
		}

	}
}
