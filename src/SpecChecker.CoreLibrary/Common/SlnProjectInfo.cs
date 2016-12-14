using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.Common
{
	/// <summary>
	/// 从 sln 文件中解析出来的项目文件信息
	/// </summary>
	public sealed class SlnProjectInfo
	{
		/// <summary>
		/// 项目的显示名称
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// 项目文件的相对路径（相对于解决方案文件）
		/// </summary>
		public string File { get; set; }
		
		/// <summary>
		/// 项目GUID
		/// </summary>
		public string Guid { get; set; }


		/// <summary>
		/// 项目当前的编译模式，是Release，还是 Debug
		/// </summary>
		public string CompilerMode { get; set; }

		/// <summary>
		/// 编译输出目录（完整路径）
		/// </summary>
		public string OutputPath { get; set; }



		/// <summary>
		/// 项目文件的全路径（完整路径）
		/// </summary>
		public string FullPath { get; set; }
		/// <summary>
		/// 项目文件的输出程序集名称
		/// </summary>
		public string AssemblyName { get; set; }
	}


	public sealed class SlnProjectInfoEqualityComparer : IEqualityComparer<SlnProjectInfo>
	{
		public bool Equals(SlnProjectInfo x, SlnProjectInfo y)
		{
			if( x == null || y == null )
				return false;

			return x.FullPath == y.FullPath;
		}

		public int GetHashCode(SlnProjectInfo obj)
		{
			if( obj == null )
				return 0;

			return obj.FullPath.GetHashCode();
		}
	}
}
