using SharpROM.Net.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpROM.Net.Messages
{
    public interface INetworkMessage
    {
        Int32 SessionID { get; set; }
        IDescriptorData Descriptor { get; set; }
    }
}
