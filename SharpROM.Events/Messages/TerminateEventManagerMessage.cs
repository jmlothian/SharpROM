using System;
using System.Collections.Generic;
using System.Text;

namespace SharpROM.Events.Messages
{
    public class TerminateEventManagerMessage : EventMessage
    {
        public TerminateEventManagerMessage()
        {
            Priority = 100;
            DispatchOnlyToTarget = true;
            EventType = (int)EVENT_TYPE.ET_TERM;
        }
    }
}
