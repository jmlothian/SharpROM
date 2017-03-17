using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SharpROM.Core;
using SharpROM.Net.Abstract;
using SharpROM.Events.Abstract;
using SharpROM.Net.Telnet;
using SharpROM.Net.Messages;
using SharpROM.Events.Messages.Telnet;
using Microsoft.Extensions.Logging;
using SharpROM.Events.Messages;

namespace SharpROM.Net.Telnet
{
	public class TelnetSocketReceiveParser : ServerObject, ISocketReceiveParser
	{
		public IEventRoutingService eventRoutingService { get; set; }

		/// <summary>
		/// descriptorSessionId -> telopt
		/// </summary>
		public Dictionary<Int32, HashSet<byte>> TelOptsOn { get; set; }
		public Dictionary<Int32, HashSet<byte>> TelOptsOff { get; set; }
		public Dictionary<Int32, HashSet<byte>> TelOptsRequested { get; set; }
		public static Dictionary<byte, ITelOptHandler> TelOptHandlers { get; set; }
        public ILogger Logger { get; set; }
		public TelnetSocketReceiveParser(IEventRoutingService evtRoutingService, ILogger<ISocketReceiveParser> logger)
		{
            Logger = logger;
			eventRoutingService = evtRoutingService;
			TelOptsOn = new Dictionary<int, HashSet<byte>>();
			TelOptsOff = new Dictionary<int, HashSet<byte>>();
			TelOptsRequested = new Dictionary<int, HashSet<byte>>();
			TelOptHandlers = new Dictionary<byte, ITelOptHandler>();

			TelOptSGA sga = new TelOptSGA();
			TelOptHandlers[sga.Opt] = sga;

			evtRoutingService.RegisterHandler(this, typeof(ConnectUserMessage));
            evtRoutingService.RegisterHandler(this, typeof(DisconnectUserMessage));
            evtRoutingService.RegisterHandler(this, typeof(TelOptMessage));

		}
		public override bool HandleEvent(IEventMessage Message)
		{
			bool ContinueProcessing = true;
            if (Message is DisconnectUserMessage)
            {
                int SessionID = ((DisconnectUserMessage)Message).SessionID;
                GlobalOutMessage mesg = new GlobalOutMessage();
                mesg.MatchForParentType = true;
                mesg.Message = SessionID.ToString() + " has disconnected.";
                eventRoutingService.QueueEvent(mesg);
            } else
            if (Message is ConnectUserMessage)
			{
				int SessionID = ((ConnectUserMessage)Message).SessionID;
				TelOptsRequested[SessionID] = new HashSet<byte>();
				TelOptsOn[SessionID] = new HashSet<byte>();
				TelOptsOff[SessionID] = new HashSet<byte>();
				//send opts!
				foreach(KeyValuePair<byte, ITelOptHandler> handler in TelOptHandlers)
				{
					OutMessageB mesg = new OutMessageB();
					mesg.Message = new byte[] { (byte)TELOPTCODE.IAC, (byte)TELOPTCODE.WILL, handler.Key };
					mesg.Target = ((ConnectUserMessage)Message).descriptorData;
					eventRoutingService.QueueEvent(mesg);
					TelOptsRequested[((ConnectUserMessage)Message).SessionID].Add(handler.Key);
				}
				//send requires.
				foreach (KeyValuePair<byte, ITelOptHandler> handler in TelOptHandlers)
				{
					if (handler.Value.Require)
					{
						OutMessageB mesg = new OutMessageB();
						mesg.Message = new byte[] { (byte)TELOPTCODE.IAC, (byte)TELOPTCODE.DO, handler.Key };
						mesg.Target = ((ConnectUserMessage)Message).descriptorData;
						eventRoutingService.QueueEvent(mesg);
					}
				}
                GlobalOutMessage connmesg = new GlobalOutMessage();
                connmesg.MatchForParentType = true;
                connmesg.Message = SessionID.ToString() + " has connected.";
                eventRoutingService.QueueEvent(connmesg);
            } else if (Message is TelOptMessage)
			{
				TelOptMessage mesg = (TelOptMessage)Message;
				int SessionID = mesg.Descriptor.SessionId;
                Logger.LogTrace("TelOpt Request - {0} - Option - {1}", ((TelOptMessage)Message).Code, ((TelOptMessage)Message).Option);

                if (TelOptsRequested.ContainsKey(mesg.Descriptor.SessionId))
				{
					byte TelOptReply = (byte)TELOPTCODE.WONT;
					if ((mesg.Code == TELOPTCODE.WILL || mesg.Code == TELOPTCODE.DO))
					{
						if (TelOptsRequested[SessionID].Contains(mesg.Option))
						{
							//we already sent the request to turn these on, don't do anything, yay!
							TelOptsOn[SessionID].Add(mesg.Option);
							TelOptHandlers[mesg.Option].OnSet(eventRoutingService, mesg.Descriptor);
						} else
						{
							//we dont support this.  No.
							if(mesg.Code == TELOPTCODE.WILL)
							{
								TelOptReply = (byte)TELOPTCODE.DONT;
							} else
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
				} else
				{
					//we haven't gotten the connected message yet, re-queue this message with a bit of a delay and bump the priority "later"
					Message.Priority++;
					Message.ProcessBy = DateTime.Now.AddMilliseconds(20);
					eventRoutingService.QueueEvent(Message);
				}
			}
			return ContinueProcessing;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="receiveDescriptor"></param>
		public byte[] Parse(byte[] data, IDescriptorData receiveDescriptor)
		{
			//new data!  Update this value with "leftovers"
			//receiveSendToken.localBuffer.dataMessageReceived
			byte[] remainingData = null;
			
			//store remaining chunks in a list, we'll concat it later
			List<byte[]> remainingDataBuffer = new List<byte[]>();
			int remainingSize = 0;
			int AtCount = 0;
			int Last = 0;
			for (int i = 0; i < data.Length; i++)
			{
				if(data[i] == (byte)TELOPTCODE.IAC)
				{
					if(i+1 < data.Length)
					{
						if(data[i+1] == (byte)TELOPTCODE.WILL
							|| data[i + 1] == (byte)TELOPTCODE.WONT
							|| data[i + 1] == (byte)TELOPTCODE.DO
							|| data[i + 1] == (byte)TELOPTCODE.DONT							
							)
						{
							if (i + 2 < data.Length)
							{
								//we ignore subnegotiation for now (we dont support anything that requires this)
								TelOptMessage mesg = new TelOptMessage();
								mesg.Code = (TELOPTCODE)data[i + 1];
								mesg.Option = data[i + 2];
								mesg.Descriptor = receiveDescriptor;
								mesg.Target = this;
								eventRoutingService.QueueEvent(mesg);
								//also, just eat any single byte operations
								i += 2;
							} else
							{
								AtCount++;
							}
						}
						else
						{
							if (AtCount > 0)
							{
								byte[] CurrentBuffer = new byte[AtCount];
								Buffer.BlockCopy(data, Last, CurrentBuffer, 0, AtCount);
								remainingSize += CurrentBuffer.Length;
								remainingDataBuffer.Add(CurrentBuffer);
							}

							Last = i + 2;
							i++;
							AtCount = 0;
						}
					} else
					{
						AtCount++;
						byte[] CurrentBuffer = new byte[AtCount];
						Buffer.BlockCopy(data, Last, CurrentBuffer, 0, AtCount);
						remainingSize += CurrentBuffer.Length;
						remainingDataBuffer.Add(CurrentBuffer);
					}
				}
				else
				{
					AtCount++;
				}
			} 
			//leave in an unfinished command, the parser after this should halt when it hits an IAC
	
			//returns unhandled data
			remainingData = new byte[remainingSize + AtCount];
			int at = 0;
			foreach(byte[] rem in remainingDataBuffer)
			{
				Buffer.BlockCopy(rem, 0, remainingData, at, rem.Length);
				at += rem.Length;
			}
			Buffer.BlockCopy(data, Last, remainingData, at, AtCount);
			if(data.Length == 1)
			{
				remainingData = data;
			}
			return remainingData;
		}
	}
}
