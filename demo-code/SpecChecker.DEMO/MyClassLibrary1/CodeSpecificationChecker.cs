using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mysoft.Map6.Core.EntityBase;
using Mysoft.Map6.Core.Pipeline.PipelineAttribute;
using Mysoft.Map6.Core.Service;
using Mysoft.Map6.Core.Common;
using Mysoft.Map6.Core.Pipeline;

// 这个文件存在新平台中，做为特殊的扩展规则
// 它在扫描工具的ScanerProxy类型的LoadCustomizeCheckerAssembly方法中加载

// 注意：命名空间和类名不能修改
// Type t = assembly.GetType("CodeSpecification.CustomizeChecker", false, false);


namespace CodeSpecification
{
	/// <summary>
	/// 用于代码规范检查的工具类，仅供运行时扫描工具调用。
	/// </summary>
	public sealed class CustomizeChecker
	{
		
		/// <summary>
		/// 检查某个类型的定义是否符合规范
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public string CheckType(Type type)
		{
			if( type.IsSubclassOf(typeof(Entity)) && type.IsSerializable == false )
				return "SPEC:RM0001; 实体类不能可序列化";

			
			return CheckServiceDefinition(type) // 服务定义
				?? CheckLazyServiceUsage(type)  // LazyService的使用方法
				?? CheckDtoDefinition(type)     // DTO定义
				?? CheckModelDefinition(type)   // 数据结构项目定义
				?? null;
			
			// TdoDescriptionAttribute
		}

	    /// <summary>
	    /// 检查某个类型定义是否符合Service规范
	    /// </summary>
	    /// <param name="type"></param>
	    /// <returns></returns>
	    private string CheckServiceDefinition(Type type)
	    {
            // 判断是否AppService
	        if (type.IsSubclassOf(typeof(AppService)))
	        {
                // 排除泛型基类
                if (type.Name.EndsWith("AppService`1") == false)  
                {
                    // AppService类必须以'AppService'结尾
                    if (type.Name.EndsWith("AppService") == false)
	                    return "SPEC:RM0002; AppService的实现类没有以AppService结尾";
                    // AppService类必须用[AppServiceScope]标记
                    if (type.GetCustomAttributes(typeof(AppServiceScopeAttribute), true).Length == 0)
	                    return "SPEC:RM0014; AppService类型必须用[AppServiceScope]标记";
	            }
	        }
            // DomainService类必须以'DomainService'结尾
            if (type.IsDomainService() && type.Name.EndsWith("DomainService") == false)
	            return "SPEC:RM0003; DomainService的实现类没有以DomainService结尾";
            // IPublicService的继承接口必须以PublicService结尾
            if (type.IsInterface && typeof(IPublicService).IsAssignableFrom(type) 
                && type.Name.EndsWith("PublicService") == false)
                return "SPEC:RM0004; IPublicService的继承接口没有以PublicService结尾";
            // EntityService的实现类必须以Service结尾
            if (type.IsClass && type.IsGenericType == false && type.IsEntityService()
                    && type.Name.EndsWith("Service") == false )
				return "SPEC:RM0005; EntityService的实现类没有以Service结尾";

            if ( typeof(IService).IsAssignableFrom(type) ) {
                // 服务类型不允许定义成静态类
                if ( type.IsSealed && type.IsAbstract )
					return "SPEC:RM0006; 服务类型不允许定义成静态类";
                // 服务类型不能定义成 封闭类
                if ( type.IsSealed )
					return "SPEC:RM0007; 服务类型不能定义成 封闭类";
			}

			return null;
		}

        /// <summary>
        /// 检查某个类型定义是否符合LazyService规范
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
		private string CheckLazyServiceUsage(Type type)
		{
            BindingFlags bindingFlags
					= BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

			PropertyInfo[] properties = type.GetProperties(bindingFlags);
			foreach( PropertyInfo p in properties ) {
				// 先分析泛型属性，只要存在就是错误的，因为LazyService<>只能用于字段
				if( p.PropertyType.IsGenericType
					&& p.PropertyType.GetGenericTypeDefinition() == typeof(LazyService<>) ) {
					return $"SPEC:RM0011; LazyService的实例只能被定义成只读的实例字段\n{p.Name}";
				}
			}


			bool isAppService = type.IsSubclassOf(typeof(AppService));
			bool isDomainService = type.IsSubclassOf(typeof(DomainService));

			// 再分析所有字段定义
			FieldInfo[] fields = type.GetFields(bindingFlags);
			foreach( FieldInfo f in fields ) {
				// 还是只分析使用LazyService<>泛型类型
				if( f.FieldType.IsGenericType
					&& f.FieldType.GetGenericTypeDefinition() == typeof(LazyService<>) ) {

					if( f.IsStatic )
						return $"SPEC:RM0011; LazyService的实例只能被定义成只读的实例字段\n{f.Name}";

					if( f.IsInitOnly == false )
						return $"SPEC:RM0011; LazyService的实例只能被定义成只读的实例字段\n{f.Name}";

					Type serviceType = f.FieldType.GetGenericArguments()[0];

					if( isAppService ) {
						if( serviceType.IsSubclassOf(typeof(AppService)) )
							return $"SPEC:RM0012; AppService之间不能相互调用\n{f.Name}";
					}
					else if( isDomainService ) {
						if( serviceType.IsSubclassOf(typeof(DomainService)) )
							return $"SPEC:RM0013; DomainService之间不能相互调用\n{f.Name}";
					}
				}
			}

			return null;
		}

        /// <summary>
        /// 检查某个类型定义是否符合DTO类型规范
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
		private string CheckDtoDefinition(Type type)
		{
			// 只分析以 Dto 结尾的类型
			if( type.Name.EndsWith("Dto", StringComparison.OrdinalIgnoreCase) == false )
				return null;


			// 新平台不支持使用字段，所以只要有定义就算是不符合规范
			FieldInfo[] fields = type.GetFields();
			if( fields.Length > 0)
				return "SPEC:RM0016; DTO类型不允许定义字段，只能定义属性";

			// 检查是否存在 DtoDescriptionAttribute 这个标记
			// 对于某些特殊情况，可以用这个标记来排除检查，
			// 所以这里只检查没有这个标记的
            object[] dtoAttrs = type.GetCustomAttributes(typeof(DtoDescriptionAttribute), false);
            if (dtoAttrs == null || dtoAttrs.Length == 0)
            {

				// 取所有属性，只要某一个是新平台的实体类型，就认为是合理的DTO类型
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

                bool includeEntity = false;
                foreach (PropertyInfo p in properties)
                {
                    if (p.PropertyType.IsSubclassOf(typeof(Entity)))
                    {
						// 找到了实体类型的属性，做个标记
                        includeEntity = true;
                        break;
                    }
                }

                if (includeEntity == false)
                    return "SPEC:RM0017; DTO必须包含实体的组合模式，不允许定义纯数据结构的类型";
            }

			return null;
		}

        /// <summary>
        /// 检查某个类型定义是否符合DTO和Enum类型规范
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
		private string CheckModelDefinition(Type type)
		{
			if( type.Assembly != typeof(Entity).Assembly ) {	// 排除map.core项目，因为它包含了一些基类定义

				if( string.IsNullOrEmpty(type.Assembly.Location) == false ) {

					if( type.Name.EndsWith("Dto", StringComparison.OrdinalIgnoreCase)
						|| type.Name.EndsWith("Enum", StringComparison.OrdinalIgnoreCase)
						|| type.IsSubclassOf(typeof(Entity))
						)
						if( type.Assembly.Location.EndsWith(".Model.dll", StringComparison.Ordinal) == false)
							return "SPEC:RM0018; DTO/Entity/Enum必须定义在以 .Model 结尾的类库项目中";
				}
			}

			return null;
		}


		/// <summary>
		/// 检查某个方法的定义是否符合规范
		/// </summary>
		/// <param name="method"></param>
		/// <returns></returns>
		public string CheckMethod(MethodInfo method)
		{
			if( typeof(IService).IsAssignableFrom(method.DeclaringType) == false )
				return null;

			if( method.IsPublic ) {
				if( method.IsVirtual == false )
					return "SPEC:RM0008; 服务类定义的公共实例方法必须是虚方法";


				if( method.IsFinal )
					return "SPEC:RM0009; 服务类定义的公共实例方法不能是封闭方法";


				//if( method.GetCustomAttribute<PipelineMethodAttribute>() == null )
				//	return "SPEC:RM0010; 服务类定义的公共实例方法必须使用PipelineMethodAttribute标记";

				if( method.DeclaringType.IsSubclassOf(typeof(AppService))
					&& method.GetCustomAttributes(typeof(ActionDescriptionAttribute), true).Length == 0
					&& method.GetCustomAttributes(typeof(ForbidHttpAttribute), true).Length == 0
					)

					return "SPEC:RM0015; AppService的方法必须用[ActionDescription]标记";
			}

			// 符合规范要求，就返回 null
			return null;
		}

		

	}
}
