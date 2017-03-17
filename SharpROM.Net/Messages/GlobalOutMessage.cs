using SharpROM.Events.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpROM.Net.Messages
{
	public class GlobalOutMessage : OutMessage
	{
		public GlobalOutMessage()
		{
			DispatchOnlyToTarget = false;
		}
	}
}
