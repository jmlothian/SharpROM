using SharpROM.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpROM.Net.Messages
{
    public class DisconnectUserMessage : EventMessage
    {
        public Int32 SessionID { get; set; }
        public DescriptorData descriptorData { get; set; }
    }
}
