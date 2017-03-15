using SharpROM.Events.Abstract;
using SharpROM.Events.Messages;
using SharpROM.Net.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpROM.Net.Telnet
{
	public class TelnetGatherTextParser : ISocketReceiveParser
	{
		public IEventRoutingService eventRoutingService
		{
			get;
			set;
		}
		public TelnetGatherTextParser(IEventRoutingService evtRoutingService)
		{
			eventRoutingService = evtRoutingService;
		}
		public byte[] Parse(byte[] data, IDescriptorData receiveDescriptor)
		{
			int AtCount = 0;
			int Last = 0;
			string Command = "";
			bool IAC_ENCOUNTERED = false;
			byte[] remaining = null;
			byte[] currentCommand = null;
			List<byte[]> Commands = new List<byte[]>();
			//lock (receiveDescriptor.CurrentCommand)
			{
				for (int i = 0; i < data.Count(); i++)
				{
					byte c = data[i];
					switch (c)
					{
						case 255:
							IAC_ENCOUNTERED = true;
							Last = i;
							break;
						case 13: //carriage return, ignore
							//we still push things onto current command
							//Command = receiveDescriptor.CurrentCommand;
							if(currentCommand == null)
							{
								currentCommand = new byte[AtCount];
							}
							if (AtCount > 0)
							{
								//byte[] CurrentBuffer = new byte[AtCount];
								Buffer.BlockCopy(data, Last, currentCommand, 0, AtCount);
								//Command += System.Text.Encoding.Default.GetString(CurrentBuffer);
							}
							//receiveDescriptor.CurrentCommand = Command;
							Last = i + 1;
							AtCount = 0;
							break;
						case 10: //newline, end of command
							//string Command = sb.ToString();
							//sb.Clear();
							//receiveDescriptor.UnprocessedCommands.Add(Command);
							Command = receiveDescriptor.CurrentCommand;
							if (AtCount > 0)
							{
								if (currentCommand == null)
								{
									currentCommand = new byte[AtCount];
									Buffer.BlockCopy(data, Last, currentCommand, 0, AtCount);
									//Command += System.Text.Encoding.Default.GetString(CurrentBuffer);
								} else
								{
									byte[] tmpCurrentCommand = new byte[currentCommand.Length + AtCount];
									Buffer.BlockCopy(currentCommand, 0, tmpCurrentCommand, 0, currentCommand.Length);
									Buffer.BlockCopy(data, Last, tmpCurrentCommand, currentCommand.Length, AtCount);
									currentCommand = tmpCurrentCommand;
								}
							}
							//blank commands are ok, usually just means "send me default view, refresh prompt, etc"
							//receiveDescriptor.UnprocessedCommands.Add(Command);

							Last = i + 1;
							AtCount = 0;
							
							GlobalOutMessage OutMesg = new GlobalOutMessage();
							OutMesg.MatchForParentType = true;
							OutMesg.Message = "[INPUT " + receiveDescriptor.SessionId.ToString() + "]" + System.Text.Encoding.ASCII.GetString(currentCommand);
							eventRoutingService.QueueEvent(OutMesg);

							//receiveDescriptor.CurrentCommand = string.Empty;
							currentCommand = null;

							break;
						default:
							AtCount++;
							break;
					}
					if (IAC_ENCOUNTERED)
						break;
				}
				//cleanup anything left in the buffer that isn't a complete command -yet-
				if (IAC_ENCOUNTERED || AtCount > 0)
				{
					byte[] CurrentBuffer = new byte[data.Length - Last];
					Buffer.BlockCopy(data, Last, CurrentBuffer, 0, (data.Length - Last));
					remaining = CurrentBuffer;
				}
			}
			return remaining;
		}
	}
}
