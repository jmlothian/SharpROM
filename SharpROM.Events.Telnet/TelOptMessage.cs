using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpROM.Net;
using SharpROM.Net.Abstract;

namespace SharpROM.Events.Messages.Telnet
{
	public class TelOptMessage : EventMessage
	{
		public TELOPTCODE Code { get; set; }
		public byte Option { get; set; }
		public IDescriptorData Descriptor { get; set; }
		public byte[] SubnegotiationData { get; set; }
	}
}
