using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpROM.Events.Abstract;

namespace SharpROM.Net.Abstract
{
	public interface ISocketReceiveParser
	{
		IEventRoutingService eventRoutingService { get; set; }
		byte[] Parse(byte[] data, IDescriptorData receiveDescriptor);

	}
}
