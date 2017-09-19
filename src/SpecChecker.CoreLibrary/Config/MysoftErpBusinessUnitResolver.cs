using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.Config
{
    /// <summary>
    /// 明源ERP专用的业务单元解析器
    /// </summary>
    public sealed class MysoftErpBusinessUnitResolver : IBusinessUnitResolver
    {
        public string GetNameByClass(string className)
        {
            if( string.IsNullOrEmpty(className) )
                return null;


            foreach( BusinessUnit unit in BusinessUnitManager.ConfingInstance.List )
                foreach( string ns in unit.Namespaces )
                    if( className.StartsWith(ns, StringComparison.OrdinalIgnoreCase) )
                        return unit.Name;

            return null;
        }

        public string GetNameByFilePath(string filePath)
        {
            if( string.IsNullOrEmpty(filePath) )
                return null;


            // 第二轮测试，用命名空间去匹配
            foreach( BusinessUnit unit in BusinessUnitManager.ConfingInstance.List )
                foreach( string ns in unit.Namespaces )
                    if( filePath.IndexOf(ns, StringComparison.OrdinalIgnoreCase) >= 0 )
                        return unit.Name;


            // 第三轮测试，用项目名称去匹配
            foreach( BusinessUnit unit in BusinessUnitManager.ConfingInstance.List )
                if( filePath.IndexOf(unit.ProjectName, StringComparison.OrdinalIgnoreCase) >= 0 )
                    return unit.Name;

            //if( filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) == false ) {
            // 第四轮测试，用模块编号去匹配
            foreach( BusinessUnit unit in BusinessUnitManager.ConfingInstance.List )
                foreach( string code in unit.FunctionCodes )
                    if( filePath.IndexOf(code, StringComparison.Ordinal) >= 0 )
                        return unit.Name;
            //}

            return null;
        }


        public string GetNameByUrl(string url)
        {
            if( string.IsNullOrEmpty(url) )
                return null;


            // 第一轮测试，用URL前缀去匹配
            foreach( BusinessUnit unit in BusinessUnitManager.ConfingInstance.List )
                foreach( string prefix in unit.UrlPrefix )
                    if( url.IndexOf(prefix, StringComparison.OrdinalIgnoreCase) >= 0 )
                        return unit.Name;



            // 第二轮测试，用命名空间去匹配
            foreach( BusinessUnit unit in BusinessUnitManager.ConfingInstance.List )
                foreach( string ns in unit.Namespaces )
                    if( url.IndexOf(ns, StringComparison.OrdinalIgnoreCase) >= 0 )
                        return unit.Name;


            // 第三轮测试，用项目名称去匹配
            foreach( BusinessUnit unit in BusinessUnitManager.ConfingInstance.List )
                if( url.IndexOf(unit.ProjectName, StringComparison.OrdinalIgnoreCase) >= 0 )
                    return unit.Name;

            // 第四轮测试，用模块编号去匹配
            foreach( BusinessUnit unit in BusinessUnitManager.ConfingInstance.List )
                foreach( string code in unit.FunctionCodes )
                    if( url.IndexOf(code, StringComparison.Ordinal) >= 0 )
                        return unit.Name;

            return null;
        }
    }
}
