using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary;
using SpecChecker.CoreLibrary.Models;

namespace SpecChecker.WebLib.Services
{
	internal static class IssueCategoryExtensions
	{
		internal static void SetIssueCategory(this IEnumerable<BaseScanResult> list)
		{
			if( list == null )
				return;

			foreach( var x in list ) {
				if( String.IsNullOrEmpty(x.RuleCode) )
					x.RuleCode = x.GetRuleCode();

				if( String.IsNullOrEmpty(x.RuleCode) == false )
					x.IssueCategory = IssueCategoryManager.GetCategoryName(x.RuleCode);

			}
		}


		internal static void SetIssueCategory(this TotalResult data)
		{
			if( data == null )
				return;

			data.RuntimeScanResults.SetIssueCategory();
			data.DbCheckResults.SetIssueCategory();
			data.JsCodeCheckResults.SetIssueCategory();
			data.CsCodeCheckResults.SetIssueCategory();
			data.ProjectCheckResults.SetIssueCategory();
			data.VsRuleCheckResults.SetIssueCategory();
			data.RuntimeScanResults.SetIssueCategory();
		}
	}
}
