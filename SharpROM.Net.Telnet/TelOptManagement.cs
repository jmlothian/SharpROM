using SharpROM.Net.Telnet.TelOptHandlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpROM.Net.Telnet
{
    public class TelOptManagement
    {
        public Dictionary<Int32, HashSet<byte>> TelOptsOn { get; set; } = new Dictionary<int, HashSet<byte>>();
        public Dictionary<Int32, HashSet<byte>> TelOptsOff { get; set; } = new Dictionary<int, HashSet<byte>>();
        public Dictionary<Int32, HashSet<byte>> TelOptsRequested { get; set; } = new Dictionary<int, HashSet<byte>>();
        public static Dictionary<byte, ITelOptHandler> TelOptHandlers { get; set; } = new Dictionary<byte, ITelOptHandler>();
        public TelOptManagement()
        {
            TelOptSGA sga = new TelOptSGA();
            TelOptHandlers[sga.Opt] = sga;
        }
    }
}
