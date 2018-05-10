using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Base;
using ClownFish.Data;
using ClownFish.Log.Model;
using ClownFish.Log.Serializer;

namespace SpecChecker.ScanLibrary.ErpLog
{
	public sealed class ExceptionLogScaner
	{
		/// <summary>
		/// 从MongoDB中读取日志数据
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		public List<ExceptionInfo> Execute(DateTime start, DateTime end, string connectionString)
		{
            throw new NotSupportedException();

			//MongoDbWriter mongo = new MongoDbWriter();
			//mongo.SetConnectionString(connectionString);

			//List<ExceptionInfo> list = mongo.GetList<ExceptionInfo>(x => x.Time >= start && x.Time < end);

			//if( list.Count > 2000 )
			//	list = list.Take(2000).ToList();
			

			//return list;
		}

		/// <summary>
		/// 从SQLSERVER中读取数据
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		public List<ExceptionInfo> Execute2(DateTime start, DateTime end, string connectionString)
		{
            List<ExceptionInfo> list = new List<ExceptionInfo>(1024);
            
            // 有些产品有多个测试环境，分别配置了不同的日志库，所以只好配置多个连接。
            // 上面的 Execute方法对应的 MongoDB版本由于目前停止使用，所以没有一起调整。


            string[] connStringArray = connectionString.SplitTrim('\r', '\n');
            foreach( string connString in connStringArray ) {

                DataTable table = null;
                using( ConnectionScope scope = ConnectionScope.Create(connString, "System.Data.SqlClient") ) {
                    // 工具最大只抓取 2000 条记录
                    string query = "select top 2000 * from LogException with(nolock) where Time >= @start and Time < @end";
                    object args = new { start, end };
                    table = CPQuery.Create(query, args).ToDataTable();
                }

                if( table == null || table.Rows.Count == 0 )
                    continue;


                foreach( DataRow row in table.Rows ) {
                    ExceptionInfo info = new ExceptionInfo();
                    LogConvert.LoadBaseInfo(row, info);

                    info.ExceptionType = row.GetString("ExceptionType") ?? string.Empty;
                    info.Exception = row.GetString("Exception") ?? string.Empty;
                    info.Addition = row.GetString("Addition");

                    info.HttpInfo = LogConvert.LoadHttpInfo(row);
                    info.BusinessInfo = LogConvert.LoadBusinessInfo(row);
                    info.SqlInfo = LogConvert.LoadSqlInfo(row);

                    list.Add(info);
                }

                if( list.Count >= 2000 )
                    break;
            }

            // 只取 2000 条数据
            if( list.Count > 2000 )
                list = list.Take(2000).ToList();
			
			return list;
		}




	}
}
