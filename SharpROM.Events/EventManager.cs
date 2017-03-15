using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SharpROM.Events.Messages;
using SharpROM.Events.Triggers;
using SharpROM.Services;
using SharpROM.Services.Events;
using SharpROM.Core.Util;
using SharpROM.Events.Abstract;
using System.Reflection;

namespace SharpROM.Events
{
    public class EventManager : IEventManager
    {
        public PriorityQueue<long, PriorityQueue<int, List<IEventMessage>>> EventQueue 
            = new PriorityQueue<long, PriorityQueue<int, List<IEventMessage>>>();
		public PriorityQueue<long, PriorityQueue<int, List<IEventMessage>>> NextQueue
			= new PriorityQueue<long, PriorityQueue<int, List<IEventMessage>>>();
        //todo: in older .Net, we used Type.GUID for this lookup - I'm not a fan of the string here, we should make something faster
        // - instead of using "Type", we should have an interface the returns a GUID that objects must implement on their own
        public SortedDictionary<string, List<IServerObject>> RegisteredHandlerObjects = new SortedDictionary<string, List<IServerObject>>();
        public SortedDictionary<string, Type> TypeLookup = new SortedDictionary<string, Type>();
        //todo: throttle ProcessQueue per update
        // also consider having an UpdatesPerSecond or something
        public int ThrottleCount = 100000;
        public bool ThrottlePerUpdate = false;
        public EventHandlerProxy ProxyHandler = null;
		public Object SyncRoot = new Object();
		public Object SyncNext = new Object();
		//private static Logger Log { get; set; }
		public EventManager()
		{
			//Log = LogManager.GetCurrentClassLogger();
		}
        public void QueueEvent(IEventMessage mesg, long eventFireBy = -1, int priority = 10)
        {
			lock (SyncNext)
			{
				if (eventFireBy == -1)
					eventFireBy = DateTime.Now.Ticks;
				if (!NextQueue.Contains(eventFireBy))
					NextQueue.Enqueue(eventFireBy, new PriorityQueue<int, List<IEventMessage>>());
				if (!NextQueue.Get(eventFireBy).ContainsKey(priority))
				{
					NextQueue.Get(eventFireBy).Enqueue(priority, new List<IEventMessage>());
				}
				NextQueue.Get(eventFireBy).Get(priority).Add(mesg);
			}
        }

		public Object RunningProcessQueue = new Object();
        public void ProcessQueue()
		{
			//only allow one call to ProcessQueue at a time per event manager
			if (Monitor.TryEnter(RunningProcessQueue))
			{
				long ProcLoops = 0;
				DateTime Starting = DateTime.Now;
				long Eventcount = 0;
				while (true)
				{


					lock (SyncRoot)
					{
						//swap queues
						if (EventQueue.Empty() && !NextQueue.Empty())
						{
							lock (SyncNext)
							{
								PriorityQueue<long, PriorityQueue<int, List<IEventMessage>>> tmpQueue = EventQueue;
								EventQueue = NextQueue;
								NextQueue = tmpQueue;
							}
						}
						ProcLoops++;
						//we only want to get things that are not scheduled too far in the future
						DateTime CurrentTime = DateTime.Now;
						//process by datetime
						
						while (!EventQueue.Empty() && EventQueue.Peek().Key <= CurrentTime.Ticks)
						{
							//pull off current item(s)
							PriorityQueue<int, List<IEventMessage>> NewMesssages = EventQueue.Dequeue().Value;
							//process by priority
							while (!NewMesssages.Empty())
							{
								List<IEventMessage> Mesgs = NewMesssages.Dequeue().Value;
								//DO EVENT HANDLING THINGS
								foreach (IEventMessage Mesg in Mesgs)
								{
									Eventcount++;
									foreach (KeyValuePair<string, List<IServerObject>> handlers in RegisteredHandlerObjects)
									{
										//match all inherited types
										if (Mesg.MatchForParentType)
										{
											if (TypeLookup[handlers.Key].IsAssignableFrom(Mesg.GetType()))
											{
												//dispatch
												Dispatch(Mesg, handlers.Value);
											}
										}
										else if (TypeLookup[handlers.Key] == Mesg.GetType()) //match specific type
										{
											//dispatch
											Dispatch(Mesg, handlers.Value);
										}
									}
								}
							}
						}
						if(Starting.AddSeconds(1) <  DateTime.Now)
						{
							//Log.Trace("Loops: " + ProcLoops.ToString() + " - Events: " + Eventcount.ToString());

							Starting = DateTime.Now;
							ProcLoops = 0;
							Eventcount = 0;
						}
						//sleep of 0 is "give other threads a chance to execute"
						// 10 seems to also give more time to do things with socket bufferss
						// 1 also seems "good enough" to give other threads a chance to do things.
						Thread.Sleep(1);
					}
				}
				Monitor.Exit(RunningProcessQueue);
			}
        }
        private bool Dispatch(IEventMessage Mesg, List<IServerObject> Objs)
		{
			bool ContinueProcessing;
			//lock (SyncRoot)
			{
				ContinueProcessing = true;
				if (Mesg.DispatchOnlyToTarget)
				{
					//mesg contains a reference, use it if we're in the right list
					if (Objs.Contains(Mesg.Target))
					{
						if (Mesg.Target.IsProxyObject)
						{
							ProxyHandler.QueueEvent(Mesg);
						}
						else
						{
							Mesg.Target.HandleEvent(Mesg);
						}
					}
				}
				else
				{
					//dispatch to all or until one of them returns false to ContinueProcessing
					for (int i = 0; i < Objs.Count && ContinueProcessing == true; i++)
					{
						if (Objs[i].IsProxyObject)
						{
							//proxy messages cannot return values, they are fire-and-forget
							ProxyHandler.QueueEvent(Mesg);
						}
						else
						{
							ContinueProcessing = Objs[i].HandleEvent(Mesg);
						}
					}
				}
			}
            return ContinueProcessing;
        }
        public void RegisterHandler(IServerObject obj, Type t)
		{
			lock (SyncRoot)
			{
				if (!RegisteredHandlerObjects.ContainsKey(t.AssemblyQualifiedName))
					RegisteredHandlerObjects[t.AssemblyQualifiedName] = new List<IServerObject>();
				RegisteredHandlerObjects[t.AssemblyQualifiedName].Add(obj);

				if (!TypeLookup.ContainsKey(t.AssemblyQualifiedName))
				{
					TypeLookup[t.AssemblyQualifiedName] = t;
				}

				//reset newly added triggers to clean them up
				if (obj is Trigger)
					((Trigger)obj).Reset();
			}
        }
        //if null, we remove this object from all handlers
        public void RemoveHandler(IServerObject obj, Type t = null)
        {
			lock (SyncRoot)
			{
				if (t == null)
				{
					foreach (KeyValuePair<string, List<IServerObject>> handlers in RegisteredHandlerObjects)
					{
						if (handlers.Value.Contains(obj))
							handlers.Value.Remove(obj);
					}
				}
				else
				{
					if (RegisteredHandlerObjects.ContainsKey(t.AssemblyQualifiedName))
					{
						if (RegisteredHandlerObjects[t.AssemblyQualifiedName].Contains(obj))
							RegisteredHandlerObjects[t.AssemblyQualifiedName].Remove(obj);
					}
				}
			}
        }
    }
}
