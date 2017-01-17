using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SpecChecker.WebLib.ViewModel
{
	public class QaReportTable
	{
		public QaReportDataRow[] Rows { get; set; }

	}

	public class QaReportDataRow
	{
		public string ScanKind { get; set; }

		public QaReportDataCell[] Cells { get; set; }
	}

	public class QaReportDataCell
	{
		/// <summary>
		/// 每个格子的数据
		/// </summary>
		public int Value { get; private set; }

		/// <summary>
		/// 数据的变化趋势，1表示上升，0表示没变化，-1表示下降
		/// </summary>
		public int Direction { get; private set; }

		/// <summary>
		/// 需要直接显示的字符串
		/// </summary>
		public string HtmlText { get; set; }


		public QaReportDataCell(string text, string color)
		{
			if( string.IsNullOrEmpty(text) ) {
				this.HtmlText = "--";
				return;
			}

			if( string.IsNullOrEmpty(color) ) {
				this.HtmlText = $"<span>{text}</span>";
				return;
			}

			this.HtmlText = $"<span style=\"color: {color}\">{text}</span>";
		}

		public QaReportDataCell(int todayValue, int lastdayValue)
		{
			this.Value = todayValue;

			if( todayValue == lastdayValue )
				this.Direction = 0;
			else if( todayValue < lastdayValue )
				this.Direction = -1;
			else
				this.Direction = 1;
		}

		public string GetHtml()
		{
			if( this.HtmlText != null )
				return this.HtmlText;

			if( this.Value < 0 )
				return $"<span style=\"color: red\">ERROR</span>";

			if( this.Direction == 0 ) {
				if( this.Value  > 0 )
					return string.Format("<span style=\"color: #333\">{0}</span>", this.Value.ToString());
				else
					return string.Format("<span style=\"color: #999\">{0}</span>", this.Value.ToString());
			}
				

			if( this.Direction < 0 )
				return string.Format("<span style=\"color: green\">{0}&nbsp;↓</span>", this.Value.ToString());

			return string.Format("<span style=\"color: red\">{0}&nbsp;↑</span>", this.Value.ToString());
		}
	}
}
