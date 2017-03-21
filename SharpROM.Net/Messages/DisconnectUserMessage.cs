using SharpROM.Events;
using SharpROM.Net.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpROM.Net.Messages
{
    public class DisconnectUserMessage : EventMessage, INetworkMessage
    {
        public Int32 SessionID { get; set; }
        public IDescriptorData Descriptor { get; set; }
    }
}
