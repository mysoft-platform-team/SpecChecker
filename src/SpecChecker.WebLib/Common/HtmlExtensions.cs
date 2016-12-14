using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ClownFish.Web;
using SpecChecker.CoreLibrary.Config;

namespace SpecChecker.WebLib.Common
{
	public static class HtmlExtensions
	{
		public static IHtmlString AsFilePathHtml(string filePath)
		{
			if( string.IsNullOrEmpty(filePath) )
				return new HtmlString(string.Empty);

			string[] parts = filePath.Split('\\');

			StringBuilder sb = new StringBuilder();

			for( int i = 0; i < parts.Length; i++ ) {
				if( i > 0 ) {
					for( int j = 0; j < i * 2; j++ )
						sb.Append("&nbsp;");
				}

				sb.Append(parts[i])
					//.Append(i < parts.Length -1 ? "\\" : "")
					.Append("<br />");
			}

			return new HtmlString(sb.ToString());
		}

		public static IHtmlString ToHtml(this string text)
		{
			return new HtmlString(System.Web.HttpUtility.HtmlEncode(text));
		}


		public static IHtmlString GetCodeCoverGraphCellTable(int total, int passed, int tableWidth=200)
		{
			string html = null;

			if( passed == 0 ) {
				html = $@"
<table cellpadding=""0"" cellspacing=""0"" style=""height:14px""><tbody><tr>
	  <td class=""graphBarNotVisited"" width=""{tableWidth}"">.</td>
</tr></tbody></table>";
			}
			else if( passed == total ) {
				html = $@"
<table cellpadding=""0"" cellspacing=""0"" style=""height:14px""><tbody><tr>
	  <td class=""graphBarVisited"" width=""{tableWidth}"">.</td>
</tr></tbody></table>";
			}
			else {
				double r = (double)passed / total;

				int a = (int)(tableWidth * r);
				if( a < 1 )
					a = 1;
				int b = tableWidth - a;

				html = $@"
<table cellpadding=""0"" cellspacing=""0"" style=""height:14px""><tbody><tr>
	  <td class=""graphBarVisited"" width=""{a}"">.</td>
	  <td class=""graphBarNotVisited"" width=""{b}"">.</td>
</tr></tbody></table>";
			}

			return new HtmlString(html);
		}


		public static bool CanLinkToDailyReportPage(this BranchSettings branch, string day)
		{
			DateTime date;
			if( DateTime.TryParse(day, out date) == false )
				return false;

			string filename = ScanResultCache.GetTotalResultFilePath(branch.Id, date);
			return File.Exists(filename);
		}

	}
}
