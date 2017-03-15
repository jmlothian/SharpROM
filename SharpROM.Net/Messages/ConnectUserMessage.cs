using SharpROM.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpROM.Net.Messages
{
	public class ConnectUserMessage : EventMessage
	{
		public Int32 SessionID { get; set; }
		public DescriptorData descriptorData { get; set; }
	}
}
