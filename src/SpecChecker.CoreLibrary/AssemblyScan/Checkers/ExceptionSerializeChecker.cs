using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Serialization;

namespace SpecChecker.CoreLibrary.AssemblyScan.Checkers
{
	internal class ExceptionSerializeChecker : ITypeCheck
	{

		public void Check(Type type, List<AssemblyScanResult> result)
		{
			// 只检查异常的定义是否可序列化
			if( typeof(Exception).IsAssignableFrom(type) == false )
				return;

			if (type.IsSerializable() == false) {
				result.Add(new AssemblyScanResult{
					Type = type,
					Message = "SPEC:R00009; 异常类型不是可序列化的"
				});

				return;
			}

			ConstructorInfo ctor = type.GetConstructor(
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
				new Type[]{typeof (SerializationInfo), typeof (StreamingContext)}, null);

			if( ctor == null )
				result.Add(new AssemblyScanResult {
					Type = type,
					Message = "SPEC:R00010; 异常类型没有定义必要的反序列化构造函数"
				});

		}

	}
}
