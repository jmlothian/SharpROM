using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpROM.Events.Messages;
using SharpROM.Events.Abstract;

namespace SharpROM.Events.Triggers.TriggerConditions
{
    class TCMatchTypeAndTargetType : TCMatchType
    {
        public Type TargetType = null;
        public override bool Matches(IEventMessage e)
        {
            if (e.GetType() == MatchType && e.Target.GetType() == TargetType)
            {
                Matched = true;
            }
            return Matched;
        }
    }
}
