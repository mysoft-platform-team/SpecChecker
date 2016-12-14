using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Web;

namespace SpecChecker.CoreLibrary.AssemblyScan
{
	internal static class SomeExtensions
	{
		/// <summary>
		/// 默认的反射扫描标记，只扫描类型定义的（不含继承）所有实例和静态的公开及非公开成员。
		/// </summary>
		internal static readonly BindingFlags DefaultBindingFlags =
			BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;



		public static bool IsLetterOrDigit(this Char c)
		{
			if( c >= '0' && c <= '9' )
				return true;

			if( c >= 'a' && c <= 'z' )
				return true;

			if( c >= 'A' && c <= 'Z' )
				return true;

			if( c == '`' )
				return true;

			return false;
		}

		public static bool IsUpper(this Char c)
		{
			return c >= 'A' && c <= 'Z';
		}

		public static string GetMethodShowInfo(this MethodInfo method)
		{
			if( method == null )
				throw new ArgumentNullException("method");

			StringBuilder sb = new StringBuilder("(");

			foreach( ParameterInfo p2 in method.GetParameters() )
				sb.Append(p2.Name).Append(",");

			if( sb.Length > 1 )
				sb.Remove(sb.Length - 1, 1);

			return method.Name + sb.ToString() + ")";
		}

		public static List<MethodInfo> GetRealMethods(this Type type)
		{
			if( type == null )
				throw new ArgumentNullException("type");

			EventInfo[] events = type.GetEvents(SomeExtensions.DefaultBindingFlags);
			PropertyInfo[] properties = type.GetProperties(SomeExtensions.DefaultBindingFlags);

			return (from m in type.GetMethods(SomeExtensions.DefaultBindingFlags)
					// 排除自动生成的代码，比如成对的GET, SET访问器
					where m.GetCustomAttribute<CompilerGeneratedAttribute>() == null
						// 排除 GET, SET 访问器
						  && properties.FirstOrDefault(p => m.Name == "get_" + p.Name || m.Name == "set_" + p.Name) == null
						// 事件订阅的内部实现方法
						  && events.FirstOrDefault(e => m.Name == "add_" + e.Name || m.Name == "remove_" + e.Name) == null
					select m).ToList();
		}


		public static List<FieldInfo> GetRealFields(this Type type)
		{
			if( type == null )
				throw new ArgumentNullException("type");

			EventInfo[] events = type.GetEvents(SomeExtensions.DefaultBindingFlags);

			return (from f in type.GetFields(SomeExtensions.DefaultBindingFlags)
					// 排除自动生成的字段，例如自动生成的属性字段
					where f.GetCustomAttribute<CompilerGeneratedAttribute>() == null
						//  排除事件的委托实例字段
						  && events.FirstOrDefault(e => e.Name == f.Name) == null
					select f).ToList();
		}


		public static bool IsSerializable(this Type type)
		{
			if( type == null )
				throw new ArgumentNullException("type");

			if( type.IsGenericType ) {

				// 如果是一个开放泛型，就直接判断IsSerializable
				if( type.IsGenericTypeDefinition )
					return type.IsSerializable;


				// 如果是泛型类型，就要保证泛型定义和类型参数都是可序列化的
				Type typeDefinition = type.GetGenericTypeDefinition();

				if( typeDefinition.IsSerializable == false )
					return false;

				// 检查每个类型参数。
				Type[] typeParameters = type.GetGenericArguments();
				foreach( Type p in typeParameters )
					if( p.IsSerializable() == false )
						return false;

				return true;
			}
			else if( type.HasElementType ) {
				// 如果是数组类型，就判断数组的元素是否支持可序列化
				return type.GetElementType().IsSerializable();
			}
			else
				return type.IsSerializable;
		}
	}
}
