using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.WebLib.ViewModel
{
	public sealed class DailyReportIndexViewModel
	{
		public string Today { get; set; }

		//public List<Tuple<string, string>> List { get; set; }
						

		public QaReportTable QaReportTable { get; set; }


	}
}
