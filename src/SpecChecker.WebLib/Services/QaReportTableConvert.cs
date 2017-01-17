using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Base.Xml;
using ClownFish.Web;
using SpecChecker.WebLib.ViewModel;

namespace SpecChecker.WebLib.Services
{
	/// <summary>
	/// 将扫描结果转换成QA要求的报表结构，
	/// 这里的转换是没有规律的，基本上是按照许畅的报表模板来呈现数据
	/// </summary>
	internal class QaReportTableConvert
	{
		/// <summary>
		/// 当天的小组汇总数据
		/// </summary>
		public List<GroupDailySummary2> TodaySummary { get; set; }

		/// <summary>
		/// 前一天的小组汇总数据
		/// </summary>
		public List<GroupDailySummary2> LastdaySummary { get; set; }


		// 顺序必须和页面输出上的标题一致
		internal static readonly string[] s_groupNames =
			(from x in SpecChecker.CoreLibrary.Config.BranchManager.ConfingInstance.Branchs
			 select x.Name).ToArray();


		private static readonly Dictionary<string, string> s_propertyDict = new Dictionary<string, string>(){
			{"安全规则", "Security"},
			{"高性能规则", "Performance"},
			{"稳定性规则", "Stability"},
			{"数据库设计", "Database"},
			{"项目设置", "Project"},
			{"ERP特殊规则", "ErpRule"},
			{"命名规则", "ObjectName"},
			{"托管规则", "VsRule"},
			{IssueCategoryManager.DefaultCategory, "Others"},
			{"基础问题小计", "1"},
			{"注释规则", "Comment"},
			{"单测用例通过率", "UnitTest"},
			{"单测代码覆盖率", "CodeCover"},
			{"性能日志", "PerformanceLog"},
			{"异常日志", "ExceptionLog"}
			  };

		
		private bool _isExistUnitTestData = false;
		

		public QaReportTable ToTableData()
		{
			if( this.TodaySummary == null || this.LastdaySummary == null )
				return null;

			_isExistUnitTestData = IsExistUnitTestData();
			//s_groupNames = (from x in this.TodaySummary select x.GroupName).ToArray();

			string[] scanKinds = (from x in s_propertyDict select x.Key).ToArray();

			QaReportTable table = new QaReportTable();
			table.Rows = new QaReportDataRow[scanKinds.Length];

			int i = -1;
			foreach(string kind in scanKinds ) {
				i++;
				QaReportDataRow row = new QaReportDataRow();
				table.Rows[i] = row;

				row.ScanKind = kind;
				row.Cells = new QaReportDataCell[s_groupNames.Length];


				if( kind == "基础问题小计" ) {
					for( int j = 0; j < s_groupNames.Length; j++ )
						row.Cells[j] = CreateTotalCell(s_groupNames[j]);
				}
				else if( kind == "单测用例通过率" ) {
					CreateUnitTestRow(row);
				}
				else if( kind == "单测代码覆盖率" ) {
					CreateCodeCoverRow(row);
				}
				else {
					for( int j = 0; j < s_groupNames.Length; j++ ) 
						row.Cells[j] = CreateCell(kind, s_groupNames[j]);
				}
			}

			return table;
		}




		private QaReportDataCell CreateCell(string scanKind, string groupName)
		{
			string propertyName = s_propertyDict[scanKind];
			PropertyInfo property = typeof(TotalSummary2).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);


			GroupDailySummary2 todaySummary = this.TodaySummary.Find(x => x.GroupName == groupName);
			int todayValue = todaySummary?.Data != null ? (int)property.GetValue(todaySummary.Data, null) : 0;


			GroupDailySummary2 lastdaySummary = this.LastdaySummary.Find(x => x.GroupName == groupName);

			int lastdayValue = lastdaySummary?.Data != null ? (int)property.GetValue(lastdaySummary.Data, null) : 0;

			return new QaReportDataCell(todayValue, lastdayValue);
		}


		//private QaReportDataCell CreateTotalCell(string groupName)
		//{
		//	GroupDailySummary2 todaySummary = this.TodaySummary.Find(x => x.GroupName == groupName);
		//    int todayValue = 0;
		//          if (todaySummary?.Data != null)
		//    {
		//        todayValue =
		//            todaySummary.Data.RuntimeScan + todaySummary.Data.DatabaseScan
		//            + todaySummary.Data.JsCodeScan + todaySummary.Data.CsCodeScan
		//            + todaySummary.Data.ProjectScan + todaySummary.Data.VsRuleScan;
		//    }

		//    GroupDailySummary2 lastdaySummary = this.LastdaySummary.Find(x => x.GroupName == groupName);
		//    int lastdayValue = 0;
		//    if (lastdaySummary?.Data != null)
		//    {
		//        lastdayValue =
		//            lastdaySummary.Data.RuntimeScan + lastdaySummary.Data.DatabaseScan
		//            + lastdaySummary.Data.JsCodeScan + lastdaySummary.Data.CsCodeScan
		//            + lastdaySummary.Data.ProjectScan + lastdaySummary.Data.VsRuleScan;
		//    }
		//    return new QaReportDataCell(todayValue, lastdayValue);
		//}


		private QaReportDataCell CreateTotalCell(string groupName)
		{
			GroupDailySummary2 todaySummary = this.TodaySummary.Find(x => x.GroupName == groupName);
			int todayValue = 0;
			if( todaySummary?.Data != null ) {
				todayValue =
					todaySummary.Data.Security + todaySummary.Data.Performance
					+ todaySummary.Data.Stability + todaySummary.Data.Database
					+ todaySummary.Data.Project + todaySummary.Data.ErpRule
					+ todaySummary.Data.ObjectName + todaySummary.Data.VsRule
					+ todaySummary.Data.Others;
			}

			GroupDailySummary2 lastdaySummary = this.LastdaySummary.Find(x => x.GroupName == groupName);
			int lastdayValue = 0;
			if( lastdaySummary?.Data != null ) {
				lastdayValue =
					lastdaySummary.Data.Security + lastdaySummary.Data.Performance
					+ lastdaySummary.Data.Stability + lastdaySummary.Data.Database
					+ lastdaySummary.Data.Project + lastdaySummary.Data.ErpRule
					+ lastdaySummary.Data.ObjectName + lastdaySummary.Data.VsRule
					+ lastdaySummary.Data.Others;
			}
			return new QaReportDataCell(todayValue, lastdayValue);
		}


		private bool IsExistUnitTestData()
		{
			foreach( var data in this.TodaySummary ) {
				if( data.Data != null && data.Data.UnitTestTotal > 0 ) {
					return true;
				}
			}

			return false;
		}
		

		private void CreateUnitTestRow(QaReportDataRow row)
		{
			// 增加单元测试结果行
			for( int j = 0; j < s_groupNames.Length; j++ ) {
				GroupDailySummary2 summary = this.TodaySummary.FirstOrDefault(x => x.GroupName == s_groupNames[j]);

				if( summary != null ) {
					string text = $"{summary.Data.UnitTestPassed}/{summary.Data.UnitTestTotal}";

					if( summary.Data.UnitTestTotal < 0 )
						row.Cells[j] = new QaReportDataCell("ERROR", "red");

					else if( summary.Data.UnitTestTotal == 0 ) {
						if( _isExistUnitTestData )
							row.Cells[j] = new QaReportDataCell("0", "red");
						else
							row.Cells[j] = new QaReportDataCell("--", "#999");
					}

					else {
						if( summary.Data.UnitTestPassed == summary.Data.UnitTestTotal )
							row.Cells[j] = new QaReportDataCell(text, "green");

						else // 如果单元测试不能 100% 通过，就用红字显示
							row.Cells[j] = new QaReportDataCell(text, "red");
					}
				}
				else {
					row.Cells[j] = new QaReportDataCell("--", "#999");
				}
			}
		}


		private void CreateCodeCoverRow(QaReportDataRow row)
		{
			// 增加单元测试结果行
			for( int j = 0; j < s_groupNames.Length; j++ ) {
				GroupDailySummary2 summary = this.TodaySummary.FirstOrDefault(x => x.GroupName == s_groupNames[j]);

				if( summary != null ) {
					string text = summary.Data.CodeCover.ToString() + "%";

					if( summary.Data.CodeCover < 0 )
						row.Cells[j] = new QaReportDataCell("ERROR", "red");

					else if( summary.Data.CodeCover == 0 ) {
						if( _isExistUnitTestData )
							row.Cells[j] = new QaReportDataCell("0", "red");
						else
							row.Cells[j] = new QaReportDataCell("--", "#999");
					}


					else if( summary.Data.CodeCover < 60 )
						row.Cells[j] = new QaReportDataCell(text, "red");

					else if( summary.Data.CodeCover >= 80 )
						row.Cells[j] = new QaReportDataCell(text, "green");

					else
						row.Cells[j] = new QaReportDataCell(text, null);
				}
				else {
					row.Cells[j] = new QaReportDataCell("--", "#999");
				}
			}
		}

	}
}
