using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Web;

namespace SpecChecker.CoreLibrary.AssemblyScan.Checkers
{
	internal class BaseClassChecker : ITypeCheck
	{
		public void Check(Type type, List<AssemblyScanResult> result)
		{
			if( typeof(IHttpHandler).IsAssignableFrom(type) 
				&& type.Name.EndsWith("Handler") == false
				//&& type.Name.IndexOf("Page") < 0
				&& type.IsSubclassOf(typeof(System.Web.UI.Page)) == false
				&& type.IsSubclassOf(typeof(HttpApplication)) == false
				) 
				result.Add(new AssemblyScanResult {
					Type = type,
					Message = "SPEC:R00001; IHttpHandler的实现类型必须以Handler结尾"
				});

			if( typeof(IHttpModule).IsAssignableFrom(type) && type.Name.EndsWith("Module") == false )
				result.Add(new AssemblyScanResult {
					Type = type,
					Message = "SPEC:R00002; IHttpModule的实现类型必须以Module结尾"
				});

			if( typeof(EventArgs).IsAssignableFrom(type) 
				&& type.Name.EndsWith("EventArgs") == false
				&& type.IsGenericType == false
				)
				result.Add(new AssemblyScanResult {
					Type = type,
					Message = "SPEC:R00003; EventArgs的派生类型必须以EventArgs结尾"
				});

			if( typeof(Exception).IsAssignableFrom(type) && type.Name.EndsWith("Exception") == false )
				result.Add(new AssemblyScanResult {
					Type = type,
					Message = "SPEC:R00004; Exception的派生类型必须以Exception结尾"
				});

			if( typeof(Attribute).IsAssignableFrom(type) && type.Name.EndsWith("Attribute") == false )
				result.Add(new AssemblyScanResult {
					Type = type,
					Message = "SPEC:R00005; Attribute的派生类型必须以Attribute结尾"
				});
		}

	}
}
