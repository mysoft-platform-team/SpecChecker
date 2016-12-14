using System;
using System.Collections.Generic;
using System.Linq;
using ClownFish.Base.WebClient;

namespace SpecChecker.WebLib.ViewModel
{
    public class PagedList<T>
	{
		public PagedList(List<T> list, int? pageIndex)
		{
			if( list == null )
				throw new ArgumentNullException("list");

			this.Total = list.Count;
			this.PageIndex = pageIndex;

			if( pageIndex.HasValue ) {
				this.List = list.Skip(pageIndex.Value * PageSize).Take(PageSize).ToList();
				this.HasNextPage = (PageIndex.Value + 1) * PageSize < list.Count;
			}
			else {
				this.List = list;
				this.HasNextPage = false;
			}
		}

		/// <summary>
		/// 当前页面要显示的数据列表
		/// </summary>
		public List<T> List { get; private set; }

		/// <summary>
		/// 分页序号（如果不指定表示不分页）
		/// </summary>
		public int? PageIndex { get; private set; }

		/// <summary>
		/// 列表总数量
		/// </summary>
		public int Total { get; private set; }

		public readonly int PageSize = 100;

		/// <summary>
		/// 是否有【下一页】
		/// </summary>
		public bool HasNextPage { get; private set; }

		/// <summary>
		/// 分支ID
		/// </summary>
		public int BranchId { get; internal set; }
		/// <summary>
		/// 报告日期
		/// </summary>
		public string Today { get; internal set; }

		/// <summary>
		/// 当前显示的数据类别
		/// </summary>
		public string DataFlag { get; internal set; }


		public string GetSortLink(string sortField)
		{
			FormDataCollection form = new FormDataCollection();
			form.AddString("id", this.BranchId.ToString())
				.AddString("day", this.Today)
				.AddString("flag", this.DataFlag)
				.AddString("sort", sortField);

			return "/ajax/scan/Result/ShowScanResult.ppx?" + form.ToString();
		}


		public string GetNextLink()
		{
			FormDataCollection form = new FormDataCollection();
			form.AddString("id", this.BranchId.ToString())
				.AddString("day", this.Today)
				.AddString("flag", this.DataFlag)
				.AddString("page", (PageIndex.GetValueOrDefault() + 1).ToString());

			return "/ajax/scan/Result/ShowScanResult.ppx?" + form.ToString();
		}
	}
}
