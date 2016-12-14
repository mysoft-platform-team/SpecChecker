using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TucaoClient.Win32
{
	/// <summary>
	/// 保证程序只运行一次的工具类
	/// </summary>
	internal static class RunOnce
	{
		private static Mutex s_mutex = null;

		public static bool CheckApplicationIsRunning()
		{
			string name = Environment.CommandLine + AppDomain.CurrentDomain.BaseDirectory;
			Mutex mutex = new Mutex(true, Md5(name));

			bool ok = false;

			for( int i = 0; i < 10; i++ ) {
				try {
					ok = mutex.WaitOne(i * 300, false);
				}
				catch { }

				if( ok )
					break;
			}

			if( ok ) {
				s_mutex = mutex;

				AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
				
				return false;
			}

			return true;
		}

		private static string Md5(string text)
		{
			if( text == null )
				text = "";

			byte[] bb = (new MD5CryptoServiceProvider()).ComputeHash(Encoding.UTF8.GetBytes(text));
			return BitConverter.ToString(bb).Replace("-", "");
		}

		private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
		{
			if( s_mutex != null ) {
				s_mutex.ReleaseMutex();
				s_mutex.Close();
			}
		}

	}
}
