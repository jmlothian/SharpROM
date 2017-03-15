using SharpROM.Events.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpROM.Events
{
    public class EventMessage : IEventMessage
    {
        //auto-increment ID to uniquely identify a message?
		public int ID { get; set; }
		public DateTime ProcessBy { get; set; }
		public int Priority { get; set; }
        [Flags]
        public enum EVENT_TYPE { ET_NONE=0 };
		public IServerObject Sender { get; set; }
		public IServerObject Target { get; set; }
        //use "is" instead of GetType so it will match all parent types as well
		public bool MatchForParentType { get; set; }
		public bool DispatchOnlyToTarget { get; set; }
		public EventMessage()
		{
			ProcessBy = DateTime.Now;
			Priority = 20;
			MatchForParentType = false;
			DispatchOnlyToTarget = false;
		}
        public virtual int[] Serialize()
        {
            int[] data = new int[1];
            return data;
        }
    }
}

//triggers are IServerObjects that handle events and do special things, such as fire composite events
// otherwise, IServerObjects (such as entities) subscribe to events directly, in either a broadcast or "directed" manner
// maybe triggers can also register / unregister other objects as event handlers?