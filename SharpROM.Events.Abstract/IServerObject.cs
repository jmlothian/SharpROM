using System;

namespace SharpROM.Events.Abstract
{
    public interface IServerObject
    {
        Guid InstanceId { get; set; }
        bool HandleEvent(IEventMessage Message);
        bool IsProxyObject { get; set; }
        //ID of controller responsible for updating this object
        int ControllerID { get; set; }

    }
}