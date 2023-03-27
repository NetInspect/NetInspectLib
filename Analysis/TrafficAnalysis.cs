using System;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Net;
using System.Timers;
using NetInspectLib.Networking.Utilities;
using NetInspectLib.Types;

namespace NetInspectLib.Analysis
{
    public class TrafficAnalysis
    {
        private readonly string _adapterName;
        private readonly System.Timers.Timer _timer;

        public event EventHandler<TrafficEventArgs> TrafficUpdated;

        public TrafficAnalysis(string adapterName)
        {
            _adapterName = adapterName;
            _timer = new System.Timers.Timer(1000); // Update every 1 second
            _timer.Elapsed += TimerElapsed;
        }

        public void Start()
        {
            Debug.WriteLine("Timer Started");
            _timer.Start();
        }

        public void Stop()
        {
            Debug.WriteLine("Timer Stopped");
            _timer.Stop();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            var bytesSent = GetTotalBytesSent();
            var bytesReceived = GetTotalBytesReceived();
            var activeConnections = GetActiveConnections();
            var eventArgs = new TrafficEventArgs(bytesSent, bytesReceived, activeConnections);
            TrafficUpdated?.Invoke(this, eventArgs);
        }

        private long GetTotalBytesSent()
        {
            var adapter = GetAdapter();
            return adapter.GetIPv4Statistics().BytesSent;
        }

        private long GetTotalBytesReceived()
        {
            var adapter = GetAdapter();
            return adapter.GetIPv4Statistics().BytesReceived;
        }

        private List<Connection> GetActiveConnections()
        {
            var connections = new List<Connection>();
            var activeConnections = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
            foreach (var connection in activeConnections)
            {
                var processName = GetProcessNameByPort(connection.LocalEndPoint.Port);
                connections.Add(
                    new Connection(
                        connection.LocalEndPoint,
                        connection.RemoteEndPoint,
                        connection.State,
                        processName
                    )
                );
            }
            return connections;
        }

        private NetworkInterface GetAdapter()
        {
            var adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var adapter in adapters)
            {
                if (adapter.Name == _adapterName)
                {
                    return adapter;
                }
            }
            throw new ArgumentException($"Adapter {_adapterName} not found.");
        }

        private string GetProcessNameByPort(int port)
        {
            var processId = PortUtility.GetProcessIdByPort(port);
            if (processId == -1)
            {
                return "-";
            }
            var process = Process.GetProcessById(processId);
            return process.ProcessName;
        }
    }

    public class TrafficEventArgs : EventArgs
    {
        public long BytesSent { get; }
        public long BytesReceived { get; }
        public List<Connection> ActiveConnections { get; }

        public TrafficEventArgs(long bytesSent, long bytesReceived, List<Connection> activeConnections)
        {
            BytesSent = bytesSent;
            BytesReceived = bytesReceived;
            ActiveConnections = activeConnections;
        }
    }
}