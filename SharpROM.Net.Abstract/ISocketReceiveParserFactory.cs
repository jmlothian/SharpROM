using SharpROM.Events.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpROM.Net.Abstract
{
    public interface ISocketReceiveParserFactory
    {
        List<ISocketReceiveParser> CreateSocketReceiveParsers(IEventRoutingService eventRoutingService);
    }
}
