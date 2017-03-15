using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpROM.Events.Messages;
using SharpROM.Core.Util;
using SharpROM.Events.Abstract;

namespace SharpROM.Events.Triggers.TriggerConditions
{
    class TCTypeCounterAndTargetType : TCMatchTypeAndTargetType
    {
        public int Counter = 0;
        public int CounterResetTo = 0;
        public int CountCompare = 0;

        public COMPARE_TYPE CompareType = COMPARE_TYPE.EQ;

        public override bool Matches(IEventMessage e)
        {
            if (e.GetType() == MatchType && e.Target.GetType() == TargetType)
            {
                Counter++;
                switch (CompareType)
                {
                    case COMPARE_TYPE.NEQ:
                        if (Counter != CountCompare)
                            Matched = true;
                        break;
                    case COMPARE_TYPE.EQ:
                        if (Counter == CountCompare)
                            Matched = true;
                        break;
                    case COMPARE_TYPE.LT:
                        if (Counter < CountCompare)
                            Matched = true;
                        break;
                    case COMPARE_TYPE.GT:
                        if (Counter > CountCompare)
                            Matched = true;
                        break;
                    case COMPARE_TYPE.LT_EQ:
                        if (Counter <= CountCompare)
                            Matched = true;
                        break;
                    case COMPARE_TYPE.GT_EQ:
                        if (Counter >= CountCompare)
                            Matched = true;
                        break;
                    default:
                        break;
                }
            }
            return Matched;
        }
        public override void Reset()
        {
            Matched = false;
            Counter = CounterResetTo;
        }
    }
}
