using SharpROM.Net.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using SharpROM.Net;
using SharpROM.Net.Abstract;

namespace SharpROM.Events.Messages.Telnet
{
    public class TelnetTextInput : EventMessage, INetworkMessage
    {
        public string Message { get; set; } = "";
        public int SessionID { get; set; } = 0;
        public IDescriptorData Descriptor { get; set; }
    }
}
