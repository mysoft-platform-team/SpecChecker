using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Log.Model;

namespace SpecChecker.WebLib.ViewModel
{
	public class PerformanceIndexViewModel : PagedList<PerformanceInfo>
	{
		public PerformanceIndexViewModel(List<PerformanceInfo> list, int? pageIndex)
			: base(list, pageIndex)
		{
		}

		public string GetDetailLink(Guid g)
		{
			return string.Format("/Performance/{0}/{1}/{2}.phtml", this.BranchId, Today, g);
		}
	}
}
