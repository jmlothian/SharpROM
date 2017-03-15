using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpROM.Net.Util
{
	public class OutputBuffer
	{
		public Int32 SentBytes { get; set; }
		Byte[] _BufferData;
		public Byte[] BufferData
		{
			get
			{
				return _BufferData;
			}
			set
			{
				_BufferData = value;
				BytesRemaining = value.Length;
				SentBytes = 0;
			}
		}
		public Int32 BytesRemaining { get; set; }
	}
}
