using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Log.Model;
using SpecChecker.CoreLibrary.Config;
using SpecChecker.CoreLibrary.Models;

namespace SpecChecker.CoreLibrary
{
	public sealed class TotalResult
	{
		/// <summary>
		/// 数据格式版本（升级数据格式后，为了区分老版本）
		/// </summary>
		public string Version { get; set; }
		/// <summary>
		/// 报告日期
		/// </summary>
		public string Today { get; set; }

		/// <summary>
		/// 分支的环境信息
		/// </summary>
		public BranchSettings Branch { get; set; }

        /// <summary>
        /// 项目编译是否通过
        /// </summary>
        public bool? BuildIsOK { get; set; }


        /// <summary>
        /// 编译源代码时产生的错误消息（兼容老版本而保留）
        /// </summary>
        public string CompilerError { get; set; }

		// 说明：最早规范检查由多种形式构成，程序集扫描直接放到ERP站点，称为运行时扫描，所以命令时采用 RuntimeScan
		//      这里为了兼容以前的数据文件，保留了这个名称，但是类型名称做了调整。

		/// <summary>
		/// 程序集扫描结果
		/// </summary>
		public List<AssemblyScanResult> RuntimeScanResults { get; set; } = new List<AssemblyScanResult>();

		public string RuntimeScanException { get; set; }

		/// <summary>
		/// 数据库扫描结果
		/// </summary>
		public List<DbCheckResult> DbCheckResults { get; set; } = new List<DbCheckResult>();

		public string DbCheckException { get; set; }

		/// <summary>
		/// 代码检查结果（服务端）
		/// </summary>
		public List<CodeCheckResult> CsCodeCheckResults { get; set; } = new List<CodeCheckResult>();

		/// <summary>
		/// 代码检查结果（前端）
		/// </summary>
		public List<CodeCheckResult> JsCodeCheckResults { get; set; } = new List<CodeCheckResult>();

		public string CodeCheckException { get; set; }


		/// <summary>
		/// 项目设置扫描结果
		/// </summary>
		public List<ProjectCheckResult> ProjectCheckResults { get; set; } = new List<ProjectCheckResult>();

		public string ProjectCheckException { get; set; }

		/// <summary>
		/// 异常日志结果
		/// </summary>
		public List<ExceptionInfo> ExceptionInfos { get; set; } = new List<ExceptionInfo>();

		public string ExceptionLogException { get; set; }


		/// <summary>
		/// 性能日志结果
		/// </summary>
		public List<PerformanceInfo> PerformanceInfos { get; set; } = new List<PerformanceInfo>();

		public string PerformanceLogException { get; set; }

		/// <summary>
		/// VS规则检查结果
		/// </summary>
		public List<VsRuleCheckResult> VsRuleCheckResults { get; set; } = new List<VsRuleCheckResult>();

		public string VsRuleCheckException { get; set; }


		/// <summary>
		/// 单元测试数据，通过数量
		/// </summary>
		public int UnitTestPassed { get; set; }

		/// <summary>
		/// 单元测试数据， 用例数量
		/// </summary>
		public int UnitTestTotal { get; set; }

		/// <summary>
		/// 单元测试用例通过率结果
		/// </summary>
		public List<UnitTestResult> UnitTestResults { get; set; } = new List<UnitTestResult>();

		/// <summary>
		/// 单元测试，代码覆盖率
		/// </summary>
		public int CodeCover { get; set; }


		/// <summary>
		/// 单元测试代码覆盖率结果
		/// </summary>
		public List<UnitTestResult> CodeCoverResults { get; set; } = new List<UnitTestResult>();


		/// <summary>
		/// 分类汇总结果
		/// </summary>
		public List<SubTotalResult> Summary { get; set; }

		/// <summary>
		/// 注释类的扫描结果数量，从CsCodeCheckResults中将SPEC:C00025;SPEC:C00028;的结果分离出来，
		/// 注意：扫描工具不填充这个属性，在网站接收扫描结果时计算结果并赋值
		/// </summary>
		public int CommentScanResultCount { get; set; }

		/// <summary>
		/// 控制台界面的输出文本
		/// </summary>
		public string ConsoleText { get; set; }

		/// <summary>
		/// 计算并赋值CommentScanResultCount属性
		/// </summary>
		/// <returns></returns>
		public int EvalCommentScanResultCount()
		{
			if( this.CsCodeCheckResults != null )
				// 将注释类的扫描结果独立出来，供报表显示
				this.CommentScanResultCount = 
						(from x in this.CsCodeCheckResults
							where x.Reason.StartsWith("SPEC:C00025;", StringComparison.Ordinal)
									|| x.Reason.StartsWith("SPEC:C00028;", StringComparison.Ordinal)
							select x
							).Count();

			return this.CommentScanResultCount;
		}


		public List<CodeCheckResult> GetCsCodeCheckResults()
		{
			return (from x in this.CsCodeCheckResults
					where x.Reason.StartsWith("SPEC:C00025;", StringComparison.Ordinal) == false 
						 && x.Reason.StartsWith("SPEC:C00028;", StringComparison.Ordinal) == false
					select x).ToList();
		}


		public List<CodeCheckResult> GetCommentScanResults()
		{
			return (from x in this.CsCodeCheckResults
					where x.Reason.StartsWith("SPEC:C00025;", StringComparison.Ordinal)
						 || x.Reason.StartsWith("SPEC:C00028;", StringComparison.Ordinal)
					select x ).ToList();
		}


		public double GetVersinNumber()
		{
			return double.Parse(this.Version);
		}
	}
}
