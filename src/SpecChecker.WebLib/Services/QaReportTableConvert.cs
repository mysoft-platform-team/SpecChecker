using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.WebLib.ViewModel;

namespace SpecChecker.WebLib.Services
{
	/// <summary>
	/// 将扫描结果转换成QA要求的报表结构
	/// </summary>
	internal class QaReportTableConvert
	{
		/// <summary>
		/// 当天的小组汇总数据
		/// </summary>
		public List<GroupDailySummary> TodaySummary { get; set; }

		/// <summary>
		/// 前一天的小组汇总数据
		/// </summary>
		public List<GroupDailySummary> LastdaySummary { get; set; }


		// 顺序必须和页面输出上的标题一致
		internal static readonly string[] s_groupNames =
			(from x in SpecChecker.CoreLibrary.Config.BranchManager.ConfingInstance.Branchs
			 select x.Name).ToArray();

		private static readonly Dictionary<string, string> s_scanKind = new Dictionary<string,string>(){
			{"程序集扫描结果", "RuntimeScan"},
			{"数据库扫描结果", "DatabaseScan"},
			{"前端代码扫描结果", "JsCodeScan"},
			{"后端代码扫描结果", "CsCodeScan"},
			{"项目设置扫描结果", "ProjectScan"},
			{"微软规则扫描结果", "VsRuleScan"},
            {"基础问题小计", "2"},
			{"代码注释扫描", "CommentScan"},
			{"单测用例通过率", "1"},
			{"单测代码覆盖率", "CodeCover"},
			{"性能日志结果", "PerformanceLogScan"},
            {"异常日志结果", "ExceptionLogScan"}
        };

		private bool _isExistUnitTestData = false;

		public QaReportTable ToTableData()
		{
			if( this.TodaySummary == null || this.LastdaySummary == null )
				return null;

			_isExistUnitTestData = IsExistUnitTestData();
			//s_groupNames = (from x in this.TodaySummary select x.GroupName).ToArray();

			QaReportTable table = new QaReportTable();
			
			string[] scanKinds = (from x in s_scanKind select x.Key).ToArray();
			table.Rows = new QaReportDataRow[scanKinds.Length];

			for( int i = 0; i < scanKinds.Length; i++ ) {
				QaReportDataRow row = new QaReportDataRow();
				table.Rows[i] = row;

				// 按许畅的报表模板，去掉扫描类型名字中的“结果”2字
				row.ScanKind = scanKinds[i].EndsWith("结果")
									? scanKinds[i].Substring(0, scanKinds[i].Length - 2)
									: scanKinds[i];

				row.Cells = new QaReportDataCell[s_groupNames.Length];


				if( scanKinds[i] == "基础问题小计" ) {
					for( int j = 0; j < s_groupNames.Length; j++ )
						row.Cells[j] = CreateTotalCell(s_groupNames[j]);
				}
				else if( scanKinds[i] == "单测用例通过率" ) {
					CreateUnitTestRow(row);
				}
				else if( scanKinds[i] == "单测代码覆盖率" ) {
					CreateCodeCoverRow(row);
				}
				else {
					for( int j = 0; j < s_groupNames.Length; j++ ) 
						row.Cells[j] = CreateCell(scanKinds[i], s_groupNames[j]);
				}
			}

			return table;
		}


		private bool IsExistUnitTestData()
		{
			foreach(var data in this.TodaySummary ) {
				if( data.Data != null && data.Data.UnitTestTotal > 0) {
					return true;
				}
			}

			return false;
		}



		private void CreateUnitTestRow(QaReportDataRow row)
		{
			// 增加单元测试结果行
			for( int j = 0; j < s_groupNames.Length; j++ ) {
				GroupDailySummary summary = this.TodaySummary.FirstOrDefault(x => x.GroupName == s_groupNames[j]);

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
				GroupDailySummary summary = this.TodaySummary.FirstOrDefault(x => x.GroupName == s_groupNames[j]);

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



		private QaReportDataCell CreateCell(string scanKind, string groupName)
		{
			string propertyName = s_scanKind[scanKind];
			PropertyInfo property = typeof(TotalSummary).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);


			GroupDailySummary todaySummary = this.TodaySummary.Find(x => x.GroupName == groupName);
			int todayValue = todaySummary?.Data != null ? (int)property.GetValue(todaySummary.Data, null) : 0;


            GroupDailySummary lastdaySummary = this.LastdaySummary.Find(x => x.GroupName == groupName);

		    int lastdayValue = lastdaySummary?.Data != null ? (int) property.GetValue(lastdaySummary.Data, null) : 0;

            return new QaReportDataCell(todayValue, lastdayValue);
		}

		private QaReportDataCell CreateTotalCell(string groupName)
		{
			GroupDailySummary todaySummary = this.TodaySummary.Find(x => x.GroupName == groupName);
		    int todayValue = 0;
            if (todaySummary?.Data != null)
		    {
		        todayValue =
		            todaySummary.Data.RuntimeScan + todaySummary.Data.DatabaseScan
		            + todaySummary.Data.JsCodeScan + todaySummary.Data.CsCodeScan
		            + todaySummary.Data.ProjectScan + todaySummary.Data.VsRuleScan;
		    }

		    GroupDailySummary lastdaySummary = this.LastdaySummary.Find(x => x.GroupName == groupName);
		    int lastdayValue = 0;
		    if (lastdaySummary?.Data != null)
		    {
		        lastdayValue =
		            lastdaySummary.Data.RuntimeScan + lastdaySummary.Data.DatabaseScan
		            + lastdaySummary.Data.JsCodeScan + lastdaySummary.Data.CsCodeScan
		            + lastdaySummary.Data.ProjectScan + lastdaySummary.Data.VsRuleScan;
		    }
		    return new QaReportDataCell(todayValue, lastdayValue);
		}


	}
}
