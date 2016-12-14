using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.Common
{
	public static class DateTimeExtensions
	{

		///// <summary>
		///// 等效于：ToString("yyyy-MM-dd HH:mm:ss")
		///// </summary>
		///// <param name="day"></param>
		///// <returns></returns>
		//public static string ToTimeString(this DateTime day)
		//{
		//	return day.ToString("yyyy-MM-dd HH:mm:ss");
		//}

		/// <summary>
		/// 等效于：ToString("yyyy-MM")
		/// </summary>
		/// <param name="day"></param>
		/// <returns></returns>
		public static string ToYMString(this DateTime day)
		{
			return day.ToString("yyyy-MM");
		}
	}
}
