using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Web;
using SpecChecker.WebLib.Services;

namespace SpecChecker.WebLib.Common
{
	public class MyBaseController : BaseController
	{
		internal static readonly string SimulateCookieName = "_device";

		/// <summary>
		/// 当前请求是不是手机浏览器发起的
		/// </summary>
		/// <param name="controller"></param>
		/// <returns></returns>
		public bool IsMobileBrowser()
		{
//Android 手机，直接微信浏览
//Mozilla/5.0 (Linux; U; Android 4.4.4; zh-cn; MI NOTE LTE Build/KTU84P) AppleWebKit/533.1 (KHTML, like Gecko)Version/4.0 MQQBrowser/5.4 TBS/025489 Mobile Safari/533.1 MicroMessenger/6.3.15.49_r8aff805.760 NetType/WIFI Language/zh_CN

//Android 手机，UC浏览器
//Mozilla/5.0 (Linux; U; Android 4.4.4; zh-CN; MI NOTE LTE Build/KTU84P) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 UCBrowser/10.9.5.729 U3/0.8.0 Mobile Safari/534.30

//iPhone 手机，直接微信浏览
//Mozilla/5.0 (iPhone; CPU iPhone OS 9_3 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Mobile/13E233 MicroMessenger/6.3.15 NetType/WIFI Language/zh_CN

//iPhone 手机，UC浏览器
//Mozilla/5.0 (iPhone; CPU iPhone OS 9_3 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13E233 Safari/601.1

			string userAgent = this.HttpContext.Request.UserAgent;
            if( string.IsNullOrEmpty(userAgent) )
                return false;

			if( userAgent.IndexOf(" Android ", StringComparison.OrdinalIgnoreCase) > 0 )
				return true;

			if( userAgent.IndexOf(" iPhone ", StringComparison.OrdinalIgnoreCase) > 0 )
				return true;

			// PC上用特殊的 cookie 模拟手机浏览操作
			if( this.GetCookie(SimulateCookieName) != null )
				return true;

			return false;
		}


		public PageResult PageResult(string viewPath, object model)
		{
			bool isMobile = this.IsMobileBrowser();

			if( isMobile )
				return new PageResult("/Views/Mobile" + viewPath, model);
			else
				return new PageResult("/Views/PC" + viewPath, model);
		}
	}
}
