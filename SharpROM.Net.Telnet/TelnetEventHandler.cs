using Microsoft.Extensions.Logging;
using SharpROM.Core;
using SharpROM.Events.Abstract;
using SharpROM.Events.Messages;
using SharpROM.Events.Messages.Telnet;
using SharpROM.Net.Messages;
using SharpROM.Net.Telnet.TelOptHandlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpROM.Net.Telnet
{
    public class TelnetEventHandler : ServerObject
    {
        public IEventRoutingService eventRoutingService { get; set; }
        public TelOptManagement TelOpts { get; set; }
        public ILogger Logger { get; set; }

        public TelnetEventHandler(IEventRoutingService evtRoutingService, TelOptManagement telOpts, ILogger<TelnetEventHandler> logger)
        {
            eventRoutingService = evtRoutingService;
            TelOpts = telOpts;
            Logger = logger;

            evtRoutingService.RegisterHandler(this, typeof(ConnectUserMessage));
            evtRoutingService.RegisterHandler(this, typeof(DisconnectUserMessage));
            evtRoutingService.RegisterHandler(this, typeof(TelOptMessage));
            evtRoutingService.RegisterHandler(this, typeof(TelnetTextInput));
        }
        public override bool HandleEvent(IEventMessage Message)
        {
            bool ContinueProcessing = true;
            if (Message is DisconnectUserMessage)
            {
                /*
                int SessionID = ((DisconnectUserMessage)Message).SessionID;
                GlobalOutMessage mesg = new GlobalOutMessage();
                mesg.MatchForParentType = true;
                mesg.Message = SessionID.ToString() + " has disconnected.";
                eventRoutingService.QueueEvent(mesg);
                */
            }
            else
            if (Message is ConnectUserMessage)
            {
                int SessionID = ((ConnectUserMessage)Message).SessionID;
                TelOpts.TelOptsRequested[SessionID] = new HashSet<byte>();
                TelOpts.TelOptsOn[SessionID] = new HashSet<byte>();
                TelOpts.TelOptsOff[SessionID] = new HashSet<byte>();
                //send opts!
                foreach (KeyValuePair<byte, ITelOptHandler> handler in TelOptManagement.TelOptHandlers)
                {
                    OutMessageB mesg = new OutMessageB();
                    mesg.Message = new byte[] { (byte)TELOPTCODE.IAC, (byte)TELOPTCODE.WILL, handler.Key };
                    mesg.Target = ((ConnectUserMessage)Message).Descriptor;
                    eventRoutingService.QueueEvent(mesg);
                    TelOpts.TelOptsRequested[((ConnectUserMessage)Message).SessionID].Add(handler.Key);
                }
                //send requires.
                foreach (KeyValuePair<byte, ITelOptHandler> handler in TelOptManagement.TelOptHandlers)
                {
                    if (handler.Value.Require)
                    {
                        OutMessageB mesg = new OutMessageB();
                        mesg.Message = new byte[] { (byte)TELOPTCODE.IAC, (byte)TELOPTCODE.DO, handler.Key };
                        mesg.Target = ((ConnectUserMessage)Message).Descriptor;
                        eventRoutingService.QueueEvent(mesg);
                    }
                }
                /*
                GlobalOutMessage connmesg = new GlobalOutMessage();
                connmesg.MatchForParentType = true;
                connmesg.Message = SessionID.ToString() + " has connected.";
                eventRoutingService.QueueEvent(connmesg);
                */
            }
            else if (Message is TelOptMessage)
            {
                TelOptMessage mesg = (TelOptMessage)Message;
                int SessionID = mesg.Descriptor.SessionId;
                Logger.LogTrace("TelOpt Request - {0} - Option - {1}", ((TelOptMessage)Message).Code, ((TelOptMessage)Message).Option);

                if (TelOpts.TelOptsRequested.ContainsKey(mesg.Descriptor.SessionId))
                {
                    byte TelOptReply = (byte)TELOPTCODE.WONT;
                    if ((mesg.Code == TELOPTCODE.WILL || mesg.Code == TELOPTCODE.DO))
                    {
                        if (TelOpts.TelOptsRequested[SessionID].Contains(mesg.Option))
                        {
                            //we already sent the request to turn these on, don't do anything, yay!
                            TelOpts.TelOptsOn[SessionID].Add(mesg.Option);
                            TelOptManagement.TelOptHandlers[mesg.Option].OnSet(eventRoutingService, mesg.Descriptor);
                        }
                        else
                        {
                            //we dont support this.  No.
                            if (mesg.Code == TELOPTCODE.WILL)
                            {
                                TelOptReply = (byte)TELOPTCODE.DONT;
                            }
                            else
                            {
                                TelOptReply = (byte)TELOPTCODE.WONT;
                            }
                            OutMessageB outMesg = new OutMessageB();
                            outMesg.Message = new byte[] { (byte)TELOPTCODE.IAC, TelOptReply, mesg.Option };
                            outMesg.Target = ((TelOptMessage)Message).Descriptor;
                            eventRoutingService.QueueEvent(outMesg);
                            Logger.LogTrace("\tReply - {0} - Option - {1}", outMesg.Message[1], outMesg.Message[2]);
                        }
                    }
                }
                else
                {
                    //we haven't gotten the connected message yet, re-queue this message with a bit of a delay and bump the priority "later"
                    Message.Priority++;
                    Message.ProcessBy = DateTime.Now.AddMilliseconds(20);
                    eventRoutingService.QueueEvent(Message);
                }
            } else if(Message is TelnetTextInput)
            {
                HandleTextInput((TelnetTextInput)Message);
            }
            return ContinueProcessing;
        }

        protected virtual void HandleTextInput(TelnetTextInput Message)
        {
            //for now, just output it to everyone like a chat server
            GlobalOutMessage OutMesg = new GlobalOutMessage();
            OutMesg.MatchForParentType = true;
            OutMesg.Message = "[INPUT " + Message.SessionID.ToString() + "]" + Message.Message;
            eventRoutingService.QueueEvent(OutMesg);
        }
    }
}
