using System.Collections.Generic;
using System.Net.Sockets;
using SharpROM.Events.Abstract;
using SharpROM.Core;
using System;
using SharpROM.Net.Util;

namespace SharpROM.Net.Abstract
{
    public interface IDescriptorData : IServerObject
    {
        string CurrentCommand { get; set; }
        int SessionId { get; }
        int TokenId { get; }
        List<string> UnprocessedCommands { get; set; }
        byte[] UnprocessedRecvBytes { get; set; }
        Int32 receivedMessageBytesDoneCount { get; set; }
        OutputBuffer CurrentBuffer();
        DescriptorBuffer localBuffer { get; set; }

        void DequeBuffer();
        void EnqueueBuffer(OutputBuffer buffer);
        void Reset();
        void SendOutput(string data);
        void SendOutputB(byte[] data);
        void SetSendArgs(SocketAsyncEventArgs e);
    }
}