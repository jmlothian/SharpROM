using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpROM.Events.Messages;
using SharpROM.Events.Abstract;

namespace SharpROM.Events.Triggers
{
    public abstract class TriggerCondition
    {
        protected bool Matched = false;
        public bool Repeats = false;
        public abstract bool Matches(IEventMessage e);
        public virtual void OnMatch()
        {
        }
        public virtual void Reset()
        {
            Matched = false;
        }
    }
}
