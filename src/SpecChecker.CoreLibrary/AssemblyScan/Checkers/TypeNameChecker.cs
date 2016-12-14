using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.AssemblyScan.Checkers
{
	internal class TypeNameChecker : ITypeCheck
	{

		public void Check(Type type, List<AssemblyScanResult> result)
		{
			if( type.IsInterface ) {
				if( type.Name[0] != 'I' )
					result.Add(new AssemblyScanResult {
						Type = type,
						Message = "SPEC:R00034; 接口名称不是以 I 开头"
					});
				else {
					if( type.Name.Length < 2 )
						result.Add(new AssemblyScanResult {
							Type = type,
							Message = "SPEC:R00035; 接口名称太短（没有名称）"
						});
					else if( type.Name[1].IsUpper() == false )
						result.Add(new AssemblyScanResult {
							Type = type,
							Message = "SPEC:R00036; 类型或者接口名称的首字母不是大写开头"
						});	

				}

			}
			else {
				if( type.Name[0].IsUpper() == false )
					result.Add(new AssemblyScanResult {
						Type = type,
						Message = "SPEC:R00037; 类型或者接口名称的首字母不是大写开头"
					});	
			}

			foreach(char c in type.Name)
				if( c.IsLetterOrDigit() == false ) {
					result.Add(new AssemblyScanResult {
						Type = type,
						Message = "SPEC:R00038; 类型或者接口的名称包含字母和数字之外的字符"
					});
					break;
				}
		}
		


	}
}
