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
				int p = message.IndexOf('\n');

				// 例如下面的返回形式：
				// return $"SPEC:RM0011; LazyService的实例只能被定义成只读的实例字段\n{f.Name}";

				if( p > 0 )
					result.Add(new AssemblyScanResult {
						Type = type,
						Message = message.Substring(0, p),
						Remark = message.Substring(p + 1)
					});
				else
					result.Add(new AssemblyScanResult {
						Type = type,
						Message = message
					});
			}
		}

	}
}
