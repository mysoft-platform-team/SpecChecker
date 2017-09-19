using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.Config
{
    /// <summary>
    /// 定义业务单元的解析接口，
    /// 注意：
    /// 1、所有实现类型必须保证【线程安全】
    /// 2、配置结果的读取可以访问  BusinessUnitManager.ConfingInstance
    /// </summary>
    public interface IBusinessUnitResolver
    {
        string GetNameByClass(string className);

        string GetNameByUrl(string url);

        string GetNameByFilePath(string filePath);

    }
}
