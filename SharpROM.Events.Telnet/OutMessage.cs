using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpROM.Events.Messages.Telnet
{
	public class OutMessage : EventMessage
	{
		public string Message { get; set; }
		public OutMessage()
		{
			DispatchOnlyToTarget = true;
		}
	}
	public class OutMessageB : EventMessage
	{
		public byte[] Message { get; set; }
		public OutMessageB()
		{
			DispatchOnlyToTarget = true;
		}
	}
}
