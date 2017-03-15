using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SharpROM.Net
{
	public class SocketListenerSettings
	{
		// the maximum number of connections the sample is designed to handle simultaneously 
		public Int32 MaxConnections { get; set; }

        // this variable allows us to create some extra SAEA objects for the pool,
        // if we wish.
        public Int32 NumberOfSaeaForRecSend { get; set; }

        // max # of pending connections the listener can hold in queue
        public Int32 PendingConnectionsInQueue { get; set; }

        // tells us how many objects to put in pool for accept operations
        public Int32 MaxSimultaneousAcceptOps { get; set; }

        // buffer size to use for each socket receive operation, in bytes
        public Int32 ReceiveBufferSize { get; set; }

        // See comments in buffer manager.
        public Int32 OpsToPreAllocate { get; set; }
        public string IpAddress { get; set; }
        public Int32 Port { get; set; }

        private IPEndPoint _LocalEndPoint = null;
        // Endpoint for the listener.
        public IPEndPoint LocalEndPoint {
            get
            {
                if(_LocalEndPoint == null)
                {
                    _LocalEndPoint = new IPEndPoint(IPAddress.Parse(IpAddress), Port);
                }
                return _LocalEndPoint;
            }
        }

        /*
		public SocketListenerSettings(Int32 maxConnections, Int32 excessSaeaObjectsInPool, Int32 backlog, Int32 maxSimultaneousAcceptOps, Int32 receiveBufferSize, Int32 opsToPreAlloc, IPEndPoint theLocalEndPoint)
		{
			this.maxConnections = maxConnections;
			this.numberOfSaeaForRecSend = maxConnections + excessSaeaObjectsInPool;
			this.backlog = backlog;
			this.maxSimultaneousAcceptOps = maxSimultaneousAcceptOps;
			this.receiveBufferSize = receiveBufferSize;
			this.opsToPreAllocate = opsToPreAlloc;
			this.localEndPoint = theLocalEndPoint;
		}
        
		public Int32 MaxConnections
		{
			get
			{
				return this.maxConnections;
			}
		}
		public Int32 NumberOfSaeaForRecSend
		{
			get
			{
				return this.numberOfSaeaForRecSend;
			}
		}
		public Int32 Backlog
		{
			get
			{
				return this.backlog;
			}
		}
		public Int32 MaxAcceptOps
		{
			get
			{
				return this.maxSimultaneousAcceptOps;
			}
		}
		public Int32 BufferSize
		{
			get
			{
				return this.receiveBufferSize;
			}
		}
		public Int32 OpsToPreAllocate
		{
			get
			{
				return this.opsToPreAllocate;
			}
		}
		public IPEndPoint LocalEndPoint
		{
			get
			{
				return this.localEndPoint;
			}
		}
        */
    }    
}
