using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ClownFish.Log.Model;

namespace SpecChecker.WebLib
{
	public class ExceptionLogModule : IHttpModule
	{
		public void Dispose()
		{
		}

		public void Init(HttpApplication app)
		{
			app.Error += App_Error;
		}

		private void App_Error(object sender, EventArgs e)
		{
			HttpApplication app = (HttpApplication)sender;
			Exception ex = app.Server.GetLastError();
			if( ex != null ) {
				ExceptionInfo exceptionInfo = ExceptionInfo.Create(ex);
				ClownFish.Log.LogHelper.SyncWrite(exceptionInfo);
			}
		}
	}
}
