using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpecChecker.WebLib.ViewModel
{
	public sealed class HighchartsDataSeries
	{
		[JsonProperty("name")]
		public string Name { get; set; }


		[JsonProperty("data")]
		public List<int> Data { get; set; }
	}
}
