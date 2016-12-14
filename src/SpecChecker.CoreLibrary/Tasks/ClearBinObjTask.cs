using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecChecker.CoreLibrary.Config;

namespace SpecChecker.CoreLibrary.Tasks
{
	public sealed class ClearBinObjTask : ITask
	{
		public void Execute(TaskContext context, TaskAction action)
		{
			if( action.Items == null || action.Items.Length == 0 )
				action.Items = context.JobOption.CodePath;

			if( action.Items == null || action.Items.Length == 0 )
				return;


			FileHelper file = new FileHelper();

			foreach( string path in action.Items )
				file.ClearBinObjFiles(path);

			context.ConsoleWrite("ClearBinObjTask OK");
		}
	}
}
