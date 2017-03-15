using SharpROM.Events.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpROM.Core
{
	/// <summary>
	/// Anything that can receive an event is a IServerObject.  This includes player data, objects, npcs, and even sockets (since they receive messages for output).
	/// </summary>
	public class ServerObject : IServerObject
    {
		//is this the real object, or just a proxy?
		public bool IsProxyObject { get; set; } = false;
		//ID of controller responsible for updating this object
		public int ControllerID { get; set; } = 0;
		public Guid ID { get; set; }
		public ServerObject()
		{
			ID = Guid.NewGuid();
		}
		public virtual bool HandleEvent(IEventMessage Message)
		{
			bool ContinueProcessing = true;
			return ContinueProcessing;
		}
	}
}
