using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.ScanLibrary.AssemblyScan
{
	[Serializable]
	public  sealed class AssemblyScanException : Exception
	{
		public AssemblyScanException(string message, Exception innerException)
			: base(message, innerException)
		{
		}


		private AssemblyScanException(SerializationInfo info, StreamingContext context)
			: base(info, context)
        {
        }

	}
}
