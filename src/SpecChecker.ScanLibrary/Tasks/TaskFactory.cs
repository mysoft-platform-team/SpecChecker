using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Base;

namespace SpecChecker.ScanLibrary.Tasks
{
	internal static class TaskFactory
	{
		private static Type[] GetTaskTypes()
		{
			Assembly asm = typeof(TaskFactory).Assembly;
			return (from t in asm.GetExportedTypes()
					where typeof(ITask).IsAssignableFrom(t)
					select t).ToArray();
		}

		public static ITask CreateTask(string actionType)
		{
			Type taskType = (from t in GetTaskTypes()
							 where t.Name.EqualsIgnoreCase(actionType) || t.Name.EqualsIgnoreCase(actionType + "Task")
							 select t).FirstOrDefault();

			if( taskType == null )
				throw new NotSupportedException("不支持的任务类型：" + actionType);

			return Activator.CreateInstance(taskType) as ITask;
		}

	}
}
