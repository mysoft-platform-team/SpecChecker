using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.Config;

namespace SpecChecker.ScanLibrary.Tasks
{
	public interface ITask
	{
		void Execute(TaskContext context, TaskAction action);
	}
}
