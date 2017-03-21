using SharpROM.Events;
using SharpROM.Net.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpROM.Net.Messages
{
	public class ConnectUserMessage : EventMessage, INetworkMessage
    {
		public Int32 SessionID { get; set; }
		public IDescriptorData Descriptor { get; set; }
	}
}
