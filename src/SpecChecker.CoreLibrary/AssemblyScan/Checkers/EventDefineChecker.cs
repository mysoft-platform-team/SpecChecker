using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SpecChecker.CoreLibrary.AssemblyScan.Checkers
{
	internal class EventDefineChecker : ITypeCheck
	{

		public void Check(Type type, List<AssemblyScanResult> result)
		{
			if( type.IsClass == false )
				return;

			EventInfo[] events = type.GetEvents(SomeExtensions.DefaultBindingFlags);

			FieldInfo[] privateStaticFields = type.GetFields( BindingFlags.Static | BindingFlags.NonPublic);

			if( events.Length == 0 || privateStaticFields.Length == 0 )
				return;


			foreach( EventInfo e in events ) {
				if( e.EventHandlerType.IsGenericType == false 
					&& e.EventHandlerType.Name.EndsWith("EventHandler") == false )
					result.Add(new AssemblyScanResult {
						Type = type,
						Message = "SPEC:R00006; 事件委托必须以EventHandler结尾",
						Remark = e.Name
					});


				// 事件委托的类型检查只针对实例事件，对于静态事件来说，sender 没有意义，因此没有必须从EventHandler继承。
				if( privateStaticFields.FirstOrDefault(f => f.Name == e.Name) == null ) {

					// 微软的有些事件委托也不符合这里的规范，这里就忽略它们。
					if( e.EventHandlerType.FullName.StartsWith("System." ) )
						continue;

					// 排除一些IDE自动生成的不规范代码
					if( e.EventHandlerType.GetCustomAttribute<System.CodeDom.Compiler.GeneratedCodeAttribute>() != null )
						continue;

					if( e.EventHandlerType.IsGenericType ) {
						if( e.EventHandlerType.GetGenericTypeDefinition() != typeof(EventHandler<>) )
							result.Add(new AssemblyScanResult {
								Type = type,
								Message = "SPEC:R00007; 泛型事件委托必须基于EventHandler<TEventArgs>",
								Remark = e.Name
							});
					}
					else {
						if( typeof(EventHandler).IsAssignableFrom(e.EventHandlerType) == false )
							result.Add(new AssemblyScanResult {
								Type = type,
								Message = "SPEC:R00008; 事件委托必须基于EventHandler",
								Remark = e.Name
							});
					}
				}
			}
		}

	}
}
