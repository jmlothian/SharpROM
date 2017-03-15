using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpROM.Events.Messages;
using SharpROM.Events.Triggers;
using SharpROM.Core.Util;
using SharpROM.Events.Abstract;

namespace SharpROM.Events.Triggers.TriggerConditions
{
    public class TCComposite : TriggerCondition
    {

        public LOGIC_OP Operator = LOGIC_OP.AND;
        public bool LeftNegation = false;
        public bool RightNegation = false;
        public TriggerCondition LeftCondition = null;
        public TriggerCondition RightCondition = null;
        private bool LeftMatched = false;
        private bool RightMatched = false;
        public override bool Matches(IEventMessage e)
        {
            if (LeftCondition.Matches(e))
                LeftMatched = true;
            if (RightCondition.Matches(e))
                RightMatched = true;

            if(Operator == LOGIC_OP.AND && (CheckNeg() && CheckNeg(false)))
            {
                Matched = true;
                OnMatch();
            }
            else if (CheckNeg() || CheckNeg(false)) //must be OR op
            {
                Matched = true;
                OnMatch();
            }
            return Matched;
        }
        public bool CheckNeg(bool R=true)
        {
            if(R)
            {
                return RightNegation == true ? !RightMatched : RightMatched;
            } else
            {
                return LeftNegation == true ? !LeftMatched : LeftMatched;
            }
        }
    }
}
