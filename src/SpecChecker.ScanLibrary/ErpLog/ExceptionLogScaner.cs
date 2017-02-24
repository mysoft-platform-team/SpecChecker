using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Log;
using ClownFish.Log.Model;
using ClownFish.Log.Serializer;
using SpecChecker.CoreLibrary.Common;

namespace SpecChecker.ScanLibrary.ErpLog
{
	public sealed class ExceptionLogScaner
	{
		public List<ExceptionInfo> Execute(DateTime start, DateTime end, string connectionString)
		{
			MongoDbWriter mongo = new MongoDbWriter();
			mongo.SetConnectionString(connectionString);

			List<ExceptionInfo> list = mongo.GetList<ExceptionInfo>(x => x.Time >= start && x.Time < end);

			if( list.Count > 2000 )
				list = list.Take(2000).ToList();

			foreach( var item in list ) {
				if( item.BusinessInfo == null )
					item.BusinessInfo = new BusinessInfo();

				item.BusinessInfo.Key1 = item.GetBusinessUnitName();
			}				

			return list;
		}
	}
}
