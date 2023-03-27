using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace NetInspectLib.Types
{
    public class Connection
    {
        public IPEndPoint LocalEndPoint { get; }
        public IPEndPoint RemoteEndPoint { get; }
        public TcpState State { get; }
        public string ProcessName { get; }

        public Connection(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, TcpState state, string processName)
        {
            LocalEndPoint = localEndPoint;
            RemoteEndPoint = remoteEndPoint;
            State = state;
            ProcessName = processName;
        }
    }
}