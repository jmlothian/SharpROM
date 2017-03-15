using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpROM.Events.Messages;
using SharpROM.Events.Abstract;
using SharpROM.Core;

namespace SharpROM.Events.Triggers
{
    public class Trigger : ServerObject
    {
        public TriggerCondition Condition = null;
        public EventManager EvtManager = null;

        //force the addition of a condition
        public Trigger(TriggerCondition Cond)
            : base()
        {
            EvtManager = GlobalEventManager.EvtManager;
            Condition = Cond;
        }
        public override bool HandleEvent(IEventMessage Message)
        {
            bool ContinueProcessing = true;
            if (Condition != null && Condition.Matches(Message))
            {
                Condition.OnMatch();
                if (Condition.Repeats)
                {
                    Condition.Reset();
                }
                else
                {
                    //trigger doesn't repeat, remove it from the eventmanager, all types it could be registered for
                    try
                    {
                        EvtManager.RemoveHandler(this);
                    }
                    catch (NullReferenceException ex)
                    {
                        Console.WriteLine("EventManager not registered with Trigger.\r\n" + ex.StackTrace);
                        ContinueProcessing = false;
                    }
                }
            }
            return ContinueProcessing;
        }
        public virtual void Reset()
        {
            Condition.Reset();
        }
    }
}
