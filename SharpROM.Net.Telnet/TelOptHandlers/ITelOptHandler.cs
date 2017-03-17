using SharpROM.Events.Abstract;
using SharpROM.Net.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpROM.Net.Telnet.TelOptHandlers
{
	public interface ITelOptHandler
	{
		bool Require { get; } //sends both WILL and DO
		byte Opt { get; }
		//onset could do many things, send a message immediately to the client, send an event for something else to respond to, etc
		void OnSet(IEventRoutingService eventRoutingService, IDescriptorData descriptor);
		void HandleNegotiate(IEventRoutingService eventRoutingService);
	}
}
