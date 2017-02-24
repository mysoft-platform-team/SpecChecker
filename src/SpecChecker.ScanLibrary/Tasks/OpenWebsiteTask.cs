using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.ScanLibrary.Tasks
{
	public sealed class OpenWebsiteTask : ITask
	{
		public void Execute(TaskContext context, TaskAction action)
		{
			if( context.EnableConsoleOut == false )
				return;

			string website = ConfigurationManager.AppSettings["ServiceWebsite"];
			if( string.IsNullOrEmpty(website) )
				return;

			System.Diagnostics.Process.Start(website);
		}
	}
}
