using SharpROM.Events.Abstract;
using System;

namespace SharpROM.Events.Abstract
{
    public interface IEventRoutingService
    {
        void ClearRoutingRules();
        void AddRoutingRule(IEventRoutingRule rule);
        void QueueEvent(IEventMessage message);
        void RegisterHandler(IServerObject obj, Type t);
        void RemoveHandler(IServerObject obj, Type t = null);
    }
}
