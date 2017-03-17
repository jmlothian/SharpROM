using System;
using System.Collections.Generic;
using System.Text;
using SharpROM.Events.Abstract;
using SharpROM.Net.Abstract;
using Microsoft.Extensions.Logging;

namespace SharpROM.Net.Telnet
{
    public class TelnetSocketReceiveParserFactory : ISocketReceiveParserFactory
    {
        public TelnetSocketReceiveParserFactory(ILogger<ISocketReceiveParser> logger)
        {
            Logger = logger;
        }
        ILogger<ISocketReceiveParser> Logger { get; set; }
        public List<ISocketReceiveParser> CreateSocketReceiveParsers(IEventRoutingService eventRoutingService)
        {
            List<ISocketReceiveParser> Parsers = new List<ISocketReceiveParser>();
            TelnetSocketReceiveParser Parser = new TelnetSocketReceiveParser(eventRoutingService, Logger);
            TelnetGatherTextParser gtParser = new TelnetGatherTextParser(eventRoutingService);
            Parsers.Add(Parser);
            Parsers.Add(gtParser);
            return Parsers;
        }
    }
}
