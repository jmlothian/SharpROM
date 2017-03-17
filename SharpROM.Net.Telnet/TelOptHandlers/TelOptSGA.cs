using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpROM.Net.Telnet;
using SharpROM.Events.Abstract;
using SharpROM.Net.Abstract;

namespace SharpROM.Net.Telnet.TelOptHandlers
{
	public class TelOptSGA : ITelOptHandler
	{

		public byte Opt
		{
			get { return 3; }
		}

		public void OnSet(IEventRoutingService eventRoutingService, IDescriptorData descriptor)
		{

		}

		public void HandleNegotiate(IEventRoutingService eventRoutingService)
		{

		}

		public bool Require
		{
			get
			{
				return true;
			}
		}
	}
}
