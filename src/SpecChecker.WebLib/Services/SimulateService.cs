using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ClownFish.Web;

namespace SpecChecker.WebLib.Services
{
	public class SimulateService : BaseController
	{
		internal static readonly string SimulateCookieName = "_device";

		/// <summary>
		/// http://localhost:55768/ajax/scan/Simulate/SetDevice.ppx?type=mobile
		/// http://spec.mysoft.com.cn:55768/ajax/scan/Simulate/SetDevice.ppx?type=mobile
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public string SetDevice(string type)
		{
			if( string.Compare(type, "mobile", StringComparison.OrdinalIgnoreCase) == 0 ) {
				HttpCookie cookie = new HttpCookie(SimulateCookieName, "mobile");
				this.WriteCookie(cookie);
			}
			else {
				HttpCookie cookie = new HttpCookie(SimulateCookieName, "");
				cookie.Expires = new DateTime(2001, 1, 1);
				this.WriteCookie(cookie);
			}

			return "OK";
		}
	}
}
