using System.Collections.Concurrent;
using System.Net.Sockets;
using NetInspectLib.Discovery;
using NetInspectLib.Types;
using NetInspectLib.Networking.Utilities;

namespace NetInspectLib.Scanning
{
    public class PortScan
    {
        public List<Host> results { get; }

        public PortScan()
        {
            results = new List<Host>();
        }

        public async Task<bool> DoPortScan(string networkMask, string portRange)
        {
            var ports = PortUtility.ParsePortRange(portRange);

            ICMPScan hostScan = new ICMPScan(networkMask);
            Task<bool> scan = hostScan.DoScan();
            bool success = await scan;
            if (success)
            {
                foreach (Host host in hostScan.results)
                {
                    results.Add(ScanHost(host, ports));
                }
            }
            return true;
        }

        private Host ScanHost(Host host, IEnumerable<int> ports)
        {
            List<Thread> threads = new List<Thread>();
            ConcurrentBag<Port> openPorts = new ConcurrentBag<Port>();
            foreach (var portNum in ports)//for (int portNum = 1; portNum <= 1024; portNum++)
            {
                Thread thread = new Thread(() =>
                {
                    Port? port = ScanPort(host, portNum);
                    if (port != null)
                    {
                        openPorts.Add(port);
                    }
                });
                threads.Add(thread);
                thread.Start();
            }

            foreach (Thread thread in threads) { thread.Join(); }

            foreach (var openPort in openPorts.OrderBy(x => x.Number))
            {
                if (!host.Ports.Contains(openPort))
                {
                    host.AddPort(openPort);
                }
            }
            return host;
        }

        private Port? ScanPort(Host host, int portNum)
        {
            using (var tcpClient = new TcpClient())
            {
                tcpClient.ReceiveTimeout = 500;
                tcpClient.SendTimeout = 500;
                try
                {
                    tcpClient.Connect(host.IPAddress, portNum);
                    tcpClient.Close();
                    return new Port(portNum, PortStatus.Open);
                }
                catch (SocketException)
                {
                    tcpClient.Close();
                    return null;
                }
            }
        }
    }
}