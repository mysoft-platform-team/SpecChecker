using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Base;

namespace SpecChecker.CoreLibrary.Common
{
	/// <summary>
	/// 一个简单的文本行序列化工具，可以将对象序列化成文本行，以及反序列化
	/// </summary>
	public sealed class TextLineSerializer
	{
		/// <summary>
		/// 将一个对象序列化成一个单行的文本。
		/// 注意：
		/// 1、只处理对象的公开属性
		/// 2、不处理嵌套结构
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public string Serialize(object obj)
		{
			if( obj == null )
				throw new ArgumentNullException(nameof(obj));

			StringBuilder sb = new StringBuilder();

			PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach( PropertyInfo p in properties ) {
				object value = p.GetValue(obj);
				string textValue = value == null ? "NULL" : value.ToString();
				sb.Append($"{p.Name}={textValue};");
			}
			return sb.ToString();
		}

		/// <summary>
		/// 从一个文本字符串中反序列化对象
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="line"></param>
		/// <returns></returns>
		public T Deserialize<T>(string line) where T: new()
		{
			if( string.IsNullOrEmpty(line))
				throw new ArgumentNullException(nameof(line));

			var list = line.SplitString(';', '=');

			T obj = Activator.CreateInstance<T>();
			PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

			foreach(var kv in list ) {
				PropertyInfo p = properties.FirstOrDefault(x => x.Name.EqualsIgnoreCase(kv.Name));
				if( p == null )
					continue;       // TODO: 现在没想好该如何处理，就先忽略吧

				object value = null;
				if( kv.Value != "NULL" ) {
					value = Convert.ChangeType(kv.Value, p.PropertyType);
					p.SetValue(obj, value);
				}
			}

			return obj;
		}
	}
}
