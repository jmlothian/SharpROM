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
using SharpROM.Net.Telnet.TelOptHandlers;

namespace SharpROM.Net.Telnet
{
	public class TelnetSocketReceiveParser : ServerObject, ISocketReceiveParser
	{
		public IEventRoutingService eventRoutingService { get; set; }
        public TelOptManagement TelOpts { get; set; } = new TelOptManagement();
		/// <summary>
		/// descriptorSessionId -> telopt
		/// </summary>

        public ILogger Logger { get; set; }
		public TelnetSocketReceiveParser(IEventRoutingService evtRoutingService, ILogger<ISocketReceiveParser> logger)
		{
            Logger = logger;

			eventRoutingService = evtRoutingService;
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
                                //todo: at this point, we need to wait for the TelnetEventHandler to finish working with the event before we can return to parsing.
                                // why?  Because if something is turned on it may effect parsing.
                                // since this server doesn't currently *support* any telopts, it isn't such a big deal 
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
