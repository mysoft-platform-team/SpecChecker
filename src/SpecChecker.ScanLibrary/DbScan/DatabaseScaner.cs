using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Data;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.CoreLibrary.Models;

namespace SpecChecker.ScanLibrary.DbScan
{
	public sealed class DatabaseScaner
	{

		public List<DbCheckResult> Execute(BranchSettings branch)
		{
            string connectionString = branch.DbLocation;

            List<DbCheckResult> list = new List<DbCheckResult>(128);

			using( ConnectionScope scope = ConnectionScope.Create(connectionString, "System.Data.SqlClient") ) {
				list.AddRange(CheckPrimaryKey());
				list.AddRange(TheSimilarIndexes());
				list.AddRange(TextFieldWrongType());
				list.AddRange(CanNotUseTrigger());
				list.AddRange(CanNotUseCLRSP());
				list.AddRange(CheckEnumTextField());
				list.AddRange(CheckStoreProcedure());
				list.AddRange(CheckUserFunction());
			}
            
            return list.ExecExcludeIgnoreRules(branch);
		}
		


		/// <summary>
		/// 检查表是否已创建主键
		/// </summary>
		/// <returns>检查结果列表</returns>
		private List<DbCheckResult> CheckPrimaryKey()
		{
            return CPQuery.Create(@"
SELECT  'SPEC:D00001; 表没有指定主键(聚集索引)' AS Reason, 'SPEC:D00001' AS RuleCode,
		'表名：' + tab.name AS Informantion , tab.name as TableName
FROM    sys.tables tab
        LEFT JOIN sys.indexes idx ON idx.object_id = tab.object_id
                                        AND tab.type = 'u'
                                        AND idx.type = 1
WHERE   idx.name IS NULL
ORDER BY tab.name"
                ).ToList<DbCheckResult>();
		}

        /// <summary>
        /// 相同的索引
        /// </summary>
        /// <returns>检查结果列表</returns>
        private List<DbCheckResult> TheSimilarIndexes()
        {
            DataTable dt = CPQuery.Create(@"
SELECT  *
INTO    #temp
FROM    ( SELECT    tab.name AS TableName ,
                    idx.index_id AS IndexId ,
                    idx.name AS IndexName ,
                    idxCol.index_column_id AS IndexColumnId ,
                    col.name AS ColName
          FROM      sys.indexes idx
                    JOIN sys.index_columns idxCol ON ( idx.object_id = idxCol.object_id
                                                       AND idx.index_id = idxCol.index_id
                                                     )
                    JOIN sys.tables tab ON ( idx.object_id = tab.object_id
                                             AND tab.type = 'u'
                                           )
                    JOIN sys.columns col ON ( idx.object_id = col.object_id
                                              AND idxCol.column_id = col.column_id
                                            )
        ) a

SELECT  TableName ,
        IndexName ,
        Columns = STUFF(( SELECT    ',' + ColName
                          FROM      #temp t
                          WHERE     TableName = t1.TableName
                                    AND IndexName = t1.IndexName
                          ORDER BY  IndexColumnId
                        FOR
                          XML PATH('')
                        ), 1, 1, '')
FROM    #temp t1
GROUP BY TableName ,
        IndexName ,
        IndexId
ORDER BY TableName ,
        IndexId
DROP TABLE #temp").ToDataTable();

            List<DbCheckResult> list = new List<DbCheckResult>();

            if( dt == null )
                return list;

            string tableName = "";
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach( DataRow dr in dt.Rows ) {
                string name = dr["TableName"].ToString();
                if( tableName != name ) {
                    tableName = name;
                    dict.Clear();
                }

                string indexName = dr["IndexName"].ToString();
                string columns = dr["Columns"].ToString();
                // 如果列表中包含相同的索引,则加入到检查结果中
                KeyValuePair<string, string> pair1 = dict.FirstOrDefault(v => v.Value.StartsWith(columns));
                if( string.IsNullOrEmpty(pair1.Key) == false ) {
                    list.Add(new DbCheckResult {
                        Reason = "SPEC:D00002; 禁止定义冗余的索引",
                        RuleCode = "SPEC:D00002",
                        Informantion = string.Format("表名：{0} 索引【{1}】包含索引【{2}】", name, pair1.Key, indexName),
                        TableName = name
                    });
                    continue;
                }

                KeyValuePair<string, string> pair2 = dict.FirstOrDefault(v => columns.StartsWith(v.Value));
                // 如果索引包含列表中的某个索引,则将列表中的删除并加入到检查结果中
                if( string.IsNullOrEmpty(pair2.Key) == false ) {
                    list.Add(new DbCheckResult {
                        Reason = "SPEC:D00002; 禁止定义冗余的索引",
                        RuleCode = "SPEC:D00002",
                        Informantion = string.Format("表名：{0} 索引【{1}】包含索引【{2}】", name, indexName, pair2.Key),
                        TableName = name
                    });
                    dict.Remove(pair2.Key);
                }

                dict[indexName] = columns;
            }

            return list;
        }

		/// <summary>
		/// 文本类型必须选择带N的字段类型
		/// </summary>
		/// <returns>检查结果列表</returns>
		private List<DbCheckResult> TextFieldWrongType()
		{
			return CPQuery.Create(@"
SELECT  'SPEC:D00003; 文本字段必须选择带N的字段类型' AS Reason , 'SPEC:D00003' AS RuleCode,
        '表【' + TABLE_NAME + '】的字段【' + COLUMN_NAME + '】类型为【' + DATA_TYPE + '】' AS Informantion , TABLE_NAME as TableName
FROM    information_schema.columns
WHERE   TABLE_NAME IN ( SELECT  name
                        FROM    sys.tables
                        WHERE   type = 'u' )
        AND DATA_TYPE = 'varchar'
ORDER BY TABLE_NAME ,
        ORDINAL_POSITION").ToList<DbCheckResult>();
		}

		/// <summary>
		/// 禁止使用触发器
		/// </summary>
		/// <returns>检查结果列表</returns>
		private List<DbCheckResult> CanNotUseTrigger()
		{
			return CPQuery.Create(@"
SELECT  'SPEC:D00004; 禁止使用触发器' AS Reason , 'SPEC:D00004' AS RuleCode,
        '触发器名称：' + a.name AS Informantion   ,object_name(b.parent_obj) AS TableName
FROM  sys.triggers a , sys.sysobjects b 
WHERE a.object_id = b.id").ToList<DbCheckResult>();
		}

		/// <summary>
		/// 禁止使用CLR SP
		/// </summary>
		/// <returns>检查结果列表</returns>
		private List<DbCheckResult> CanNotUseCLRSP()
		{
			return CPQuery.Create(@"
SELECT  'SPEC:D00005;禁止使用CLR SP' AS Reason , 'SPEC:D00005' AS RuleCode,
        '程序集名称：' + name AS Informantion  , '' as TableName
FROM    sys.assemblies
WHERE   assembly_id <> 1").ToList<DbCheckResult>();
		}

		/// <summary>
		/// 枚举字段没有对应的文本字段
		/// </summary>
		/// <returns>检查结果列表</returns>
		private List<DbCheckResult> CheckEnumTextField()
		{
			return CPQuery.Create(@"
SELECT  N'SPEC:D00006; 枚举字段没有对应的文本字段' AS Reason, 'SPEC:D00006' AS RuleCode,
		N'字段名称：' + t.name  + '.' + c.name   as Informantion, t.name as TableName
        --ce.name AS EnumTextColName
FROM    sysobjects t
        INNER JOIN syscolumns c ON t.id = c.id
        LEFT JOIN syscolumns ce ON t.id = ce.id
                                   AND c.name = ce.name + 'Enum'
WHERE   t.xtype = 'U'
        AND c.name LIKE '%Enum'
		AND ce.name IS NULL").ToList<DbCheckResult>();
		}


		/// <summary>
		/// 查找所有的存储过程（禁止使用）
		/// </summary>
		/// <returns></returns>
		private List<DbCheckResult> CheckStoreProcedure()
		{
			List<string> names = CPQuery.Create(@"
select sp.name from sys.all_objects as sp
where (sp.type = N'P' or sp.type = N'PC' )and(CAST(
 case 
    when sp.is_ms_shipped = 1 then 1
    when (
        select major_id 
        from  sys.extended_properties 
        where 
            major_id = sp.object_id and 
            minor_id = 0 and 
            class = 1 and 
            name = N'microsoft_database_tools_support') 
        is not null then 1
    else 0
end          
             AS bit)=0)
order by name
").ToScalarList<string>();

			List<DbCheckResult> list = new List<DbCheckResult>();

			foreach(string name in names ) {
				DbCheckResult result = new DbCheckResult();
				result.TableName = name;
				result.Reason = "SPEC:D00007; 禁止使用存储过程";
                result.RuleCode = "SPEC:D00007";
                result.Informantion = name;
				//result.BusinessUnit = BusinessUnitManager.OthersBusinessUnitName;	// 这里不分业务单元
				list.Add(result);
			}

			return list;
		}



		/// <summary>
		/// 查找所有的自定义数据库函数（禁止使用）
		/// </summary>
		/// <returns></returns>
		private List<DbCheckResult> CheckUserFunction()
		{
			List<string> names = CPQuery.Create(@"
select sp.name from sys.all_objects as sp
where (sp.type = N'TF' or sp.type = N'FN' )and(CAST(
 case 
    when sp.is_ms_shipped = 1 then 1
    when (
        select major_id 
        from  sys.extended_properties 
        where 
            major_id = sp.object_id and 
            minor_id = 0 and 
            class = 1 and 
            name = N'microsoft_database_tools_support') 
        is not null then 1
    else 0
end          
             AS bit)=0)
order by name
").ToScalarList<string>();

			List<DbCheckResult> list = new List<DbCheckResult>();

			foreach( string name in names ) {
				DbCheckResult result = new DbCheckResult();
				result.TableName = name;
				result.Reason = "SPEC:D00008; 禁止使用自定义数据库函数";
                result.RuleCode = "SPEC:D00008";
				result.Informantion = name;
				//result.BusinessUnit = BusinessUnitManager.OthersBusinessUnitName;   // 这里不分业务单元
				list.Add(result);
			}

			return list;
		}



	}
}
