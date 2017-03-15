using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpROM.Events
{
    //lazyness
    public static class GlobalEventManager
    {
        public static EventManager EvtManager = new EventManager();
    }
}
