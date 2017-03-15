using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpROM.Events.Messages
{
	public class LogMessage : EventMessage
	{
		public string Message { get; set; }
		public DateTime Timestamp { get; set; }
	}
}
