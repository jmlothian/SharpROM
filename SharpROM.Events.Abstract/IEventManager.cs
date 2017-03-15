using System;
using SharpROM.Events.Abstract;

namespace SharpROM.Events
{
    public interface IEventManager
    {
        void QueueEvent(IEventMessage mesg, long eventFireBy = -1, int priority = 10);
        void ProcessQueue();
        void RegisterHandler(IServerObject obj, Type t);
        void RemoveHandler(IServerObject obj, Type t = null);
    }
}