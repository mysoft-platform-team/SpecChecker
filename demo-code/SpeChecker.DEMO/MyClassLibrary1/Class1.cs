using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyClassLibrary1
{
	/// <summary>
	/// Class1
	/// </summary>
	public class Class1
	{
		private static int commandTimeout = 1000 * 60 * 30; // 超时时间30分钟


		/// <summary>
		/// 执行一批SQL语句
		/// </summary>
		/// <param name="trans"></param>
		/// <param name="SQL"></param>
		/// <returns></returns>
		public static bool ExecuteSql(DbTransaction trans, string SQL)
		{
			if( trans == null )
				throw new Exception("transaction is null.");
			if( SQL.Trim() == "")
				throw new Exception("SQL is empty.");

			try {
				DbCommand cmd = trans.Connection.CreateCommand();
				cmd.Transaction = trans;
				cmd.CommandText = SQL;
				cmd.CommandTimeout = commandTimeout;
				cmd.ExecuteNonQuery();
				return true;
			}
			catch {
				return false;
			}
		}


		/// <summary>
		/// ReadFile
		/// </summary>
		/// <param name="RelativePath"></param>
		/// <returns></returns>
		public static string ReadFile(string RelativePath)
		{
			var context = HttpContext.Current;
			string path = context.Server.MapPath(RelativePath);
			if( !File.Exists(path) )
				throw new Exception("File Not Found .");

			string text = context.Cache[path] as string;
			if( text == null ) {
				text = File.ReadAllText(path);
				context.Cache[path] = text;
			}
			return text;
		}



	}
}
