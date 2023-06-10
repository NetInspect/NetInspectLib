using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetInspectLib.Discovery;
using NetInspectLib.Networking.Utilities;
using NetInspectLib.Types;

namespace NetInspectLib.Scanning
{
    public class PortScan
    {
        public List<Host> Results { get; }

        private int maxConcurrentTasks = Environment.ProcessorCount * 2;

        public PortScan()
        {
            Results = new List<Host>();
        }

        public async Task<bool> DoPortScan(string networkMask, string portRange)
        {
            var ports = PortUtility.ParsePortRange(portRange);

            ICMPScan hostScan = new ICMPScan(networkMask);
            bool success = await hostScan.DoScan();
            if (success)
            {
                var hostTasks = new List<Task<Host>>();
                foreach (Host host in hostScan.results)
                {
                    hostTasks.Add(ScanHost(host, ports));
                }
                var hosts = await Task.WhenAll(hostTasks);
                Results.AddRange(hosts);
            }
            return true;
        }

        private async Task<Host> ScanHost(Host host, IEnumerable<int> ports)
        {
            var openPorts = new ConcurrentBag<Port>();
            var portTasks = new List<Task>();
            var throttler = new SemaphoreSlim(maxConcurrentTasks);

            foreach (int portNum in ports)
            {
                await throttler.WaitAsync();
                portTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        Port? port = await ScanPort(host, portNum);
                        if (port != null)
                        {
                            openPorts.Add(port);
                        }
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }));
            }

            await Task.WhenAll(portTasks);

            foreach (var openPort in openPorts.OrderBy(x => x.Number))
            {
                if (!host.Ports.Contains(openPort))
                {
                    host.AddPort(openPort);
                }
            }
            return host;
        }

        private async Task<Port?> ScanPort(Host host, int portNum)
        {
            using (var tcpClient = new TcpClient())
            {
                tcpClient.ReceiveTimeout = 100;
                tcpClient.SendTimeout = 100;
                try
                {
                    await tcpClient.ConnectAsync(host.IPAddress, portNum);
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
