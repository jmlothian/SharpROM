using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpROM.Events;
using SharpROM.Events.Messages;
using SharpROM.Events.Abstract;
using SharpROM.Core;
using SharpROM.Net.Abstract;
using SharpROM.Net.Util;

namespace SharpROM.Net
{
	public class DescriptorSender
	{
		public DescriptorData Descriptor { get; set; }
		public string Output { get; set; }
		public void ExecuteSend(Object threadContext)
		{
			Descriptor.SendOutput(Output);
		}
	}
	public class DescriptorSenderB
	{
		public DescriptorData Descriptor { get; set; }
		public byte[] Output { get; set; }
		public void ExecuteSend(Object threadContext)
		{
			Descriptor.SendOutputB(Output);
		}
	}
	public class DescriptorData : ServerObject, IDescriptorData
    {
		//internal Mediator theMediator;
		public DescriptorBuffer localBuffer { get; set; }

		internal Int32 socketHandleNumber;

		internal readonly Int32 bufferOffsetReceive;
		internal readonly Int32 permanentReceiveMessageOffset;
		internal readonly Int32 bufferOffsetSend;

		private Int32 idOfThisObject; //for testing only        

		internal Int32 lengthOfCurrentIncomingMessage;

		//receiveMessageOffset is used to mark the byte position where the message
		//begins in the receive buffer. This value can sometimes be out of
		//bounds for the data stream just received. But, if it is out of bounds, the 
		//code will not access it.
		internal Int32 receiveMessageOffset;
		//internal Byte[] byteArrayForPrefix;
		//internal readonly Int32 receivePrefixLength;
		//internal Int32 receivedPrefixBytesDoneCount = 0;
		public Int32 receivedMessageBytesDoneCount { get; set; } = 0;
		//This variable will be needed to calculate the value of the
		//receiveMessageOffset variable in one situation. Notice that the
		//name is similar but the usage is different from the variable
		//receiveSendToken.receivePrefixBytesDone.
		//internal Int32 recPrefixBytesDoneThisOp = 0;

		internal Int32 sendBytesRemainingCount;
		//internal readonly Int32 sendPrefixLength;
		internal Queue<OutputBuffer> dataToSend { get; set; }
		internal Int32 bytesSentAlreadyCount;

		//The session ID correlates with all the data sent in a connected session.
		//It is different from the transmission ID in the DataHolder, which relates
		//to one TCP message. A connected session could have many messages, if you
		//set up your app to allow it.
		private Int32 sessionId { get; set; }
		private SocketListener SocketManager { get; set; }
		private SocketAsyncEventArgs ReceiveArgs { get; set; }
		private SocketAsyncEventArgs SendArgs { get; set; }
		public string CurrentCommand { get; set; }
		public List<string> UnprocessedCommands { get; set; }
		public byte[] UnprocessedRecvBytes { get; set; }
		public Object SyncRecvProcessing = new Object();
		public void SetSendArgs(SocketAsyncEventArgs e)
		{
			SendArgs = e;
			e.UserToken = this;
		}
		public DescriptorData(SocketListener Sock, SocketAsyncEventArgs e, Int32 rOffset, Int32 sOffset, Int32 identifier)
		{
			CurrentCommand = String.Empty;
			UnprocessedCommands = new List<string>();
			ReceiveArgs = e;
			//we'll use this to hook into sending data to a socket
			SocketManager = Sock;

			this.idOfThisObject = identifier;

			//Create a Mediator that has a reference to the SAEA object.
			//this.theMediator = new Mediator(e);
			this.bufferOffsetReceive = rOffset;
			this.bufferOffsetSend = sOffset;
			//this.receivePrefixLength = receivePrefixLength;
			//this.sendPrefixLength = sendPrefixLength;
			//this.receiveMessageOffset = rOffset + receivePrefixLength;
			this.permanentReceiveMessageOffset = this.receiveMessageOffset;
			dataToSend = new Queue<OutputBuffer>();
		}

		//Let's use an ID for this object during testing, just so we can see what
		//is happening better if we want to.
		public Int32 TokenId
		{
			get
			{
				return this.idOfThisObject;
			}
		}

		internal void CreateNewDataHolder()
		{
			localBuffer = new DescriptorBuffer();
		}

		//Used to create sessionId variable in DataHoldingUserToken.
		//Called in ProcessAccept().
		internal void CreateSessionId()
		{
			//create session id
			sessionId = Interlocked.Increment(ref SocketListener.SessionID);
		}

		public Int32 SessionId
		{
			get
			{
				return this.sessionId;
			}
		}

		public void Reset()
		{
			//this.receivedPrefixBytesDoneCount = 0;
			this.receivedMessageBytesDoneCount = 0;
			//this.recPrefixBytesDoneThisOp = 0;
			this.receiveMessageOffset = this.permanentReceiveMessageOffset;
		}


		public override bool HandleEvent(IEventMessage Message)
		{
			bool ContinueProcessing = true;
			if(Message is GlobalOutMessage)
			{
				DescriptorSender ds = new DescriptorSender{ Descriptor = this, Output=((GlobalOutMessage)Message).Message + "\r\n"};
				ThreadPool.QueueUserWorkItem(ds.ExecuteSend);
				//Thread t = new Thread(ds.ExecuteSend);
				//t.Start();
				//send out
				//SendOutput(((GlobalOutMessage)Message).Message + "\r\n");
			}
			else if (Message is OutMessage)
			{
				//send out
				//don't continue processing, this is the only target possible
				DescriptorSender ds = new DescriptorSender { Descriptor = this, Output = ((OutMessage)Message).Message };
				Thread t = new Thread(ds.ExecuteSend);
				t.Start();
				//SendOutput(((GlobalOutMessage)Message).Message);
				ContinueProcessing = false;
			} else if (Message is OutMessageB)
			{
				DescriptorSenderB ds = new DescriptorSenderB { Descriptor = this, Output = ((OutMessageB)Message).Message };
				Thread t = new Thread(ds.ExecuteSend);
				t.Start();
				//SendOutput(((GlobalOutMessage)Message).Message);
				ContinueProcessing = false;
			}
			return ContinueProcessing;
		}
		public AutoResetEvent asyncOpsAreDone = new AutoResetEvent(true);
		public Object SyncRoot = new Object();
		public void SendOutput(string data)
		{
			lock (SyncRoot)
			{
				//In this example code, we will  
				//prefix the message with the length of the message. So we put 2 
				//things into the array.
				// 1) prefix,
				// 2) the message.

				//Determine the length of the message that we will send.
				//Int32 lengthOfCurrentOutgoingMessage = data.Length;

				//convert the message to byte array
				Byte[] arrayOfBytesInMessage = Encoding.ASCII.GetBytes(data);
				OutputBuffer buffer = new OutputBuffer { SentBytes = 0, BufferData = arrayOfBytesInMessage };

				//Create the byte array to send.
				EnqueueBuffer(buffer);
				//this.dataToSend.Enqueue(buffer);
				//this.dataToSend = arrayOfBytesInMessage;

				//Now copy the 2 things to the theUserToken.dataToSend.
				//Buffer.BlockCopy(arrayOfBytesInPrefix, 0, theUserToken.dataToSend, 0, this.sendPrefixLength);
				//Buffer.BlockCopy(arrayOfBytesInMessage, 0, this.dataToSend, 0, lengthOfCurrentOutgoingMessage);

				//this.sendBytesRemainingCount = lengthOfCurrentOutgoingMessage;
				//this.bytesSentAlreadyCount = 0;
				//Console.WriteLine("SENDING FROM " + SessionId + " -> " + data);
				if (data.Length > 0)
				{
					SocketManager.StartSend(SendArgs);
				}
			}
		}
		public void SendOutputB(byte [] data)
		{
			lock (SyncRoot)
			{
				//In this example code, we will  
				//prefix the message with the length of the message. So we put 2 
				//things into the array.
				// 1) prefix,
				// 2) the message.

				//Determine the length of the message that we will send.
				//Int32 lengthOfCurrentOutgoingMessage = data.Length;

				//convert the message to byte array
				Byte[] arrayOfBytesInMessage = data;
				OutputBuffer buffer = new OutputBuffer { SentBytes = 0, BufferData = arrayOfBytesInMessage };

				//Create the byte array to send.
				EnqueueBuffer(buffer);
				//this.dataToSend.Enqueue(buffer);
				//this.dataToSend = arrayOfBytesInMessage;

				//Now copy the 2 things to the theUserToken.dataToSend.
				//Buffer.BlockCopy(arrayOfBytesInPrefix, 0, theUserToken.dataToSend, 0, this.sendPrefixLength);
				//Buffer.BlockCopy(arrayOfBytesInMessage, 0, this.dataToSend, 0, lengthOfCurrentOutgoingMessage);

				//this.sendBytesRemainingCount = lengthOfCurrentOutgoingMessage;
				//this.bytesSentAlreadyCount = 0;
				//Console.WriteLine("SENDING FROM " + SessionId + " -> " + data);
				if (data.Length > 0)
				{
					SocketManager.StartSend(SendArgs);
				}
			}
		}
		public OutputBuffer CurrentBuffer()
		{
			OutputBuffer currBuffer = null;
			lock(this.dataToSend)
			{
				if (this.dataToSend.Count > 0)
				{
					currBuffer = this.dataToSend.Peek();
				}
			}
			return currBuffer;
		}
		public void DequeBuffer()
		{
			lock(this.dataToSend)
			{
				if(this.dataToSend.Count > 0)
					this.dataToSend.Dequeue();
			}
		}
		public Object SendLock = new Object();
		public void EnqueueBuffer(OutputBuffer buffer)
		{
			lock (this.dataToSend)
			{
				this.dataToSend.Enqueue(buffer);
			}
			//	if(this.dataToSend.Count == 1)
			//	{
			//		//SocketManager.StartSend(SendArgs);
			//	}
			//}
		}
	}
}
