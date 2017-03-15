using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpROM.Events.Messages;
using SharpROM.Events.Abstract;

namespace SharpROM.Events.Triggers.TriggerConditions
{
    public class TCMatchType : TriggerCondition
    {
        protected Type MatchType = null;
        public override bool Matches(IEventMessage e)
        {
            if (e.GetType() == MatchType)
            {
                Matched = true;
            }
            return Matched;
        }
    }
}
