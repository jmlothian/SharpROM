using System;
using System.Collections.Generic;
using System.Text;
using SharpROM.Events.Abstract;
using SharpROM.Net.Abstract;

namespace SharpROM.Net.Telnet
{
    public class TelnetSocketReceiveParserFactory : ISocketReceiveParserFactory
    {
        public List<ISocketReceiveParser> CreateSocketReceiveParsers(IEventRoutingService eventRoutingService)
        {
            List<ISocketReceiveParser> Parsers = new List<ISocketReceiveParser>();
            TelnetSocketReceiveParser Parser = new TelnetSocketReceiveParser(eventRoutingService);
            TelnetGatherTextParser gtParser = new TelnetGatherTextParser(eventRoutingService);
            Parsers.Add(Parser);
            Parsers.Add(gtParser);
            return Parsers;
        }
    }
}
