using System;
using System.Collections.Generic;
using System.Text;

namespace SharpROM.Events.Messages.Telnet
{
    public class TelnetTextInput : EventMessage
    {
        public string Message { get; set; } = "";
        public int SessionId { get; set; } = 0;
    }
}
