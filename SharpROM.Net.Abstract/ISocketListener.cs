using SharpROM.Net.Util;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace SharpROM.Net.Abstract
{
    public interface ISocketListener
    {
        SocketAsyncEventArgsPool PoolOfRecSendEventArgs { get; set; }
        bool HandleRecvData(SocketAsyncEventArgs receiveSendEventArgs, IDescriptorData receiveSendToken, Int32 remainingBytesToProcess);
        void StartSend(SocketAsyncEventArgs sendEventArgs);
		List<ISocketReceiveParser> Parsers { get; set; }
    }
}