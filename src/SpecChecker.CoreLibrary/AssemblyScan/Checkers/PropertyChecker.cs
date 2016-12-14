using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.AssemblyScan.Checkers
{
	internal class PropertyChecker : ITypeCheck
	{
		public void Check(Type type, List<AssemblyScanResult> result)
		{
			PropertyInfo[] pArray = type.GetProperties(SomeExtensions.DefaultBindingFlags);

			foreach( PropertyInfo p in pArray ) {
				if( p.Name[0].IsUpper() == false )
					result.Add(new AssemblyScanResult {
						Type = type,
						Message = "SPEC:R00018; 属性名称的首字母不是大写开头",
						Remark = p.Name
					});

				if( p.CanRead == false )
					result.Add(new AssemblyScanResult {
						Type = type,
						Message = "SPEC:R00019; 不允许定义只写属性",
						Remark = p.Name
					});

			}

		}



	}
}
