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
	internal class FieldChecker : ITypeCheck
	{

		public void Check(Type type, List<AssemblyScanResult> result)
		{
			if( type.IsEnum )
				return;

			foreach( FieldInfo f in type.GetRealFields() ) {
				if (f.IsLiteral) {
					result.Add(new AssemblyScanResult {
						Type = type,
						Message = "SPEC:R00011; 不允许使用const，请用static readonly代替",
						Remark = f.Name
					});

					continue;
				}

				if( f.IsPublic || f.IsAssembly ) {
					if( f.Name[0].IsUpper() == false ) {
						//WPF的程序集中，控件都是 interrnal的，但是名字是小写开头，所以忽略这东西！
						if( f.IsAssembly && f.FieldType.IsSubclassOf(typeof(System.Windows.Controls.Control)) )
							continue;
						else {
							result.Add(new AssemblyScanResult {
								Type = type,
								Message = "SPEC:R00012; 公开（或常量）字段的名称的首字母不是大写开头",
								Remark = f.Name
							});

							continue;
						}
					}						
				}

				// Readonly 的字段先不做要求。
				if( f.IsInitOnly )
					continue;


				if( f.IsPrivate ) {
					if( f.IsStatic ) {
						if( f.Name.StartsWith("s_", StringComparison.Ordinal) == false )
							result.Add(new AssemblyScanResult {
								Type = type,
								Message = "SPEC:R00013; 私有静态字段的名称不是以 s_ 开头",
								Remark = f.Name
							});
					}
					else {
						if( f.Name[0] != '_' )
							result.Add(new AssemblyScanResult {
								Type = type,
								Message = "SPEC:R00014; 私有实例字段的名称不是以 _ 开头",
								Remark = f.Name
							});
					}
				}

			}
		}

	}
}
