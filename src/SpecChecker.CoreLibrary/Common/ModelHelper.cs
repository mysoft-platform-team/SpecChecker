﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Log.Model;
using SpecChecker.CoreLibrary.Config;

namespace SpecChecker.CoreLibrary.Common
{
	public static class ModelHelper
	{
		public static string GetBusinessUnitName(this ExceptionInfo exceptionInfo)
		{
			if( exceptionInfo.HttpInfo != null )
				return BusinessUnitManager.GetNameByUrl(exceptionInfo.HttpInfo.Url);

			return BusinessUnitManager.OthersBusinessUnitName;
		}

		public static string GetUrl(this ExceptionInfo exceptionInfo)
		{
			if( exceptionInfo.HttpInfo != null )
				return exceptionInfo.HttpInfo.Url;

			return string.Empty;
		}


		public static string GetBusinessUnitName(this PerformanceInfo performanceInfo)
		{
			if( performanceInfo.HttpInfo != null )
				return BusinessUnitManager.GetNameByUrl(performanceInfo.HttpInfo.Url);

			return BusinessUnitManager.OthersBusinessUnitName;
		}

		public static string GetUrl(this PerformanceInfo performanceInfo)
		{
			if( performanceInfo.HttpInfo != null )
				return performanceInfo.HttpInfo.Url;

			return string.Empty;
		}


		/// <summary>
		/// 获取某个分支所包含的业务单元名称
		/// </summary>
		/// <param name="branch"></param>
		/// <returns></returns>
		public static string[] GetBizUnitNames(this BranchSettings branch)
		{
			if( string.IsNullOrEmpty(branch.SubSystems) )
				return null;

			string[] subSystem = branch.SubSystems.Split(
								new char[] { ';', }, StringSplitOptions.RemoveEmptyEntries);

			return (from b in BusinessUnitManager.ConfingInstance.List
										 where subSystem.FirstOrDefault(x => x == b.SubSystem) != null
										 select b.Name).ToArray();
		}
	}
}