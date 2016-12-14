using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.UnitTest
{
	public sealed class SlnUnitTestResult
	{
		/// <summary>
		/// 单元测试用例通过率结果
		/// </summary>
		public List<UnitTestResult> UnitTestResults { get; set; }



		/// <summary>
		/// 单元测试代码覆盖率结果
		/// </summary>
		public List<UnitTestResult> CodeCoverResults { get; set; }
	}
}
