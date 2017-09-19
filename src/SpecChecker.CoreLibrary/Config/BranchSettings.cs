using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.Config
{
    [Serializable]
    public sealed class BranchSettings
    {
        #region 兼容属性
        // 为了方便其他地方使用，尤其是兼容以前版本，所以保留了Id和Name属性
        // 作业文件不需要指定，在加载时，使用作业文件中的属性来覆盖这二个属性
        // 在扫描结果中会保存到数据文件中。

        /// <summary>
        /// 分支编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 分支名称
        /// </summary>
        public string Name { get; set; }

        #endregion

        /// <summary>
        /// 数据库连接字符口串，检查数据表定义
        /// </summary>
        public string DbLocation { get; set; }

        /// <summary>
        /// MongoDB连接字符串，用于提取日志
        /// </summary>
        public string MongoLocation { get; set; }

        /// <summary>
        /// 如果操作过程出现异常，用于接收通知的邮箱地址
        /// </summary>
        public string ExceptionAlertEmail { get; set; }


        /// <summary>
        /// 要忽略的规则
        /// </summary>
        public string IgnoreRules { get; set; }

        /// <summary>
		/// 要忽略的数据库对象
		/// </summary>
		public string IgnoreDbObjects { get; set; }

        /// <summary>
        /// 包含的子系统名称（引用SpecChecker.BusinessUnitConfig.config中的SubSystem）
        /// </summary>
        public string SubSystems { get; set; }


    }
}
