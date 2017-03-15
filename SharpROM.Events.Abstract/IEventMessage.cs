using System;
using SharpROM.Events.Abstract;

namespace SharpROM.Events.Abstract
{
    public interface IEventMessage
    {
        bool DispatchOnlyToTarget { get; set; }
        int ID { get; set; }
        bool MatchForParentType { get; set; }
        int Priority { get; set; }
        DateTime ProcessBy { get; set; }
        IServerObject Sender { get; set; }
        IServerObject Target { get; set; }
        int EventType { get; set; }
        int[] Serialize();
    }
}