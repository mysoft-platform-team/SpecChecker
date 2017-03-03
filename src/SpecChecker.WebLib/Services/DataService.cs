using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Base;
using ClownFish.Base.Http;
using ClownFish.Web;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.WebLib.Common;
using SpecChecker.WebLib.ViewModel;

namespace SpecChecker.WebLib.Services
{
	public class DataService : MyBaseController
	{
		[Action(OutFormat = SerializeFormat.Json)]
		public object GetKindTotal(string flag, string kind)
		{
			if( string.IsNullOrEmpty(kind) )
				kind = "BaseTotal";

			// 1、根据参数名找到对应的属性，后续将读取这个属性的值
			PropertyInfo p = typeof(TotalSummary2).GetProperty(kind, BindingFlags.Instance | BindingFlags.Public);
			if( p == null )
				throw new ArgumentException($"kind参数值 {kind} 不是有效的属性名称。");


			// 2、确定开始和结束日期
			DateTime start, end;
			GetDateRnage(flag, out start, out end);

			// 保存有效的日期值（有数据文件）
			List<string> dayList = new List<string>();

			HighchartsDataSeries[] series = (from b in BranchManager.ConfingInstance.Branchs
											 let h = new HighchartsDataSeries {
												 Name = b.Name,
												 Data = new List<int>()
											 }
											 select h).ToArray();

			// 3、加载日期范围内的数据
			DailySummaryHelper helper = new DailySummaryHelper();
			for( ; start <= end; start = start.AddDays(1) ) {
				// 加载每一天的汇总数据
				List<GroupDailySummary2> data = helper.LoadData(start);
				if( data == null )
					continue;

				// 将日期保存下来，做为X轴
				dayList.Add(start.ToDateString());

				// 根据小组来循环匹配数据（填充每行的数据）
				foreach( var s in series ) {
					GroupDailySummary2 group = data.FirstOrDefault(x => x.GroupName == s.Name);
					if( group != null ) {
						int value = (int)p.GetValue(group.Data);
						s.Data.Add(value);
					}
					else
						s.Data.Add(0);	// 没有找到就用【零】来填充
				}
			}


			// 4、构造Highcharts控件所需要的数据对象
			return new {
				chart = new { type = "line" },
				title = new { text = "代码扫描问题趋势分析表" },
				xAxis = new {
					categories = (from x in dayList let s = x.Substring(5) select s).ToArray()
				},
				yAxis = new {
					title = new { text = "基础问题小计数量" }
				},
				series = series
			};
		}


		private void GetDateRnage(string flag, out DateTime start, out DateTime end)
		{
			end = DateTime.Today;

			switch( flag ) {
				case "2": {
						start = end.AddMonths(-1);
						break;
					}
				case "3": {
						start = end.AddMonths(-2);
						break;
					}
				case "4": {
						start = end.AddMonths(-3);
						break;
					}
				case "5": {
						start = end.AddMonths(-6);
						break;
					}
				default: {
						start = end.AddDays(-15);
						break;
					}
			}
		}

		private int GetPropertyValue(TotalSummary2 summary, string propertyName)
		{
			PropertyInfo p = typeof(TotalSummary2).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
			if( p == null )
				return 0;
			else
				return (int)p.GetValue(null);
		}
	}
}
