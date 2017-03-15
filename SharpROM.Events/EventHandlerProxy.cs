using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpROM.Events.Messages;
using SharpROM.Events.Abstract;

namespace SharpROM.Events
{
    /*
     * EventHandlerProxy is a base class to allow distributed control of IServerObjects across multiple systems/services/servers
     * 
     * External services register GameObj's as proxies with the EventManager.  These objects are only stand-in objects for 
     * GameObjs controlled elsewhere.  Their properties can be filled in (read-only by local game logic) but not acted upon.
     * 
     * Example:  Network Game Server with two "nodes" that represent nearby areas, and one "manager" that handles inter-node communication.
     * Player1 in Node1 approaches Boundary near Node2.  Node1 signals Node2 to register a proxy for Player1 (probably via an event itself)
     * Node2 creates a new Player object, assigning values it was sent, and signifying its a Proxy
     * Node2 registers the Player-proxy object with any Player related default event handlers
     * Node2 gets a registered event
     * Node2 sends it to the EventHandlerProxy
     * EventHandlerProxy sends it "out" to the manager
     * Manager sends it "in" to Node1 via proxy
     * 
     * Somewhere in this chain, the EventMessage is re-wired to contain correct target/sender data that is local to the node
     * 
     * Abstract class because this is meant to be implemented as-needed.  SendEvents will likely be custom on a case-by-case basis.
     */

    public abstract class EventHandlerProxy
    {
        //todo: include a way to throttle SendEvents, perhaps by Ticks

        //event manager this proxy is for
        EventManager EvtManager = null;
		public Object SyncRoot = new Object();

        //wire up on construction, both sides
        public EventHandlerProxy(EventManager EManager)
        {
            EvtManager = EManager;
            EvtManager.ProxyHandler = this;
        }

        Queue<IEventMessage> ProxyEventQueue = new Queue<IEventMessage>();
        public void QueueEvent(IEventMessage Mesg)
        {
            //proxy messages are always "immediate" and are all the same priority
			lock (SyncRoot)
			{
				ProxyEventQueue.Enqueue(Mesg);
			}
        }
        //send queued events to the proxied location
		//make sure to lock syncroot in all calls to this!
        public abstract void SendEvents();
        public virtual void RecvEvents(EventMessage Mesg)
        {
            //message should get here rewired already with non-proxy objects
            EvtManager.QueueEvent(Mesg);
        }
    }
}
