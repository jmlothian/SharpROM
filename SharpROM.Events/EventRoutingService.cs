using System;
using System.Collections.Generic;
using System.Threading;
using SharpROM.Events;
using SharpROM.Events.Messages;
using SharpROM.Events.Abstract;

namespace SharpROM.Services.Events.Impl
{
    public class EventRoutingService : IEventRoutingService
    {
        //private static Logger Log { get; set; }

        private List<IEventRoutingRule> RoutingRules { get; set; }
        private Dictionary<int, IEventManager> EventManagers { get; set; }

        public EventRoutingService()
        {
            //Log = LogManager.GetCurrentClassLogger();
            RoutingRules = new List<IEventRoutingRule>();
            EventManagers = new Dictionary<int, IEventManager>();
        }

        // EventHandler instances get created by routing rules as well.
        public void ClearRoutingRules()
        {
            // TODO - Destroy EventManager(s)

            // This is a TEMP HACK so that we have one EventManager at least.
            if (EventManagers.Count.Equals(1))
                EventManagers.Remove(1);
            
            RoutingRules.Clear();
        }

        public void AddRoutingRule(IEventRoutingRule rule)
        {
            //Log.Trace("Configuring event routing with rule {0}.", "PLACEHOLDER");
            
            // TODO - create EventManager(s) based on this rule.

            // This is a TEMP HACK so that we have one EventManager at least.
            if (EventManagers.Count.Equals(0))
                EventManagers.Add(1, new EventManager());

            // Saving this to a list so we can reference it later if we want.
            RoutingRules.Add(rule);

			//TODO - remove, etc, see below
			Thread t = new Thread(ProcessQueues);
			t.Start();
        }

		//TODO - remove this?  Or something?  At the very least, it needs startup/shutdown hooks, and consider thread pooling
		//  I certainly don't like firing new threads for this.  Really, this implementation probably should be abstracted as well.
		public void ProcessQueues()
		{
			//while(true)
			{
				foreach(KeyValuePair<int, IEventManager> EvtManager in EventManagers)
				{
					Thread t = new Thread(EvtManager.Value.ProcessQueue);
					t.Start();
				}
				//Thread.Sleep(0);
			}
		}

        public void QueueEvent(IEventMessage message)
        {
            //var senderId = message.Sender == null ? "NULL" : message.Sender.ID.ToString();
            //Log.Trace("Queueing event with ID {0} from sender {1}.", message.ID, senderId);
            EventManagers[1].QueueEvent(message);
        }

        public void RegisterHandler(IServerObject obj, Type t)
        {
            //Log.Trace("Registering handler for type {0} and game object with GUID {1}.", t.Name, obj.ID);
            EventManagers[1].RegisterHandler(obj, t);
        }

        public void RemoveHandler(IServerObject obj, Type t = null)
        {
            var name = t == null ? String.Empty : t.Name;
            //Log.Trace("Unregistering handler for type {0} and game object with GUID {1}.", name, obj.ID);
            EventManagers[1].RemoveHandler(obj, t);
        }
    }
}
