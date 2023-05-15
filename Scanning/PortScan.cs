using System.Collections.Concurrent;
using System.Net.Sockets;
using NetInspectLib.Discovery;
using NetInspectLib.Types;
using NetInspectLib.Networking.Utilities;

namespace NetInspectLib.Scanning
{
    /// <summary>
    /// Class for scanning Ports
    /// <example>
    /// Usage
    /// <code>
    ///     PortScan portscanner = new PortScan();
    ///     Task<bool> scan = portscanner.DoPortScan("192.168.1.0/24", "1-1000");
    ///     bool success = await portscanner;
    ///     if(success)
    ///     {
    ///         foreach(host in portscanner.results)
    ///         {
    ///             //Do Something
    ///         }
    ///     }
    /// </code>
    /// </example>
    /// </summary>
    public class PortScan
    {
        public List<Host> results { get; }

        public PortScan()
        {
            results = new List<Host>();
        }

        /// <summary>
        /// Scans a range or single IP address + a single or range of ports specified for the status of the ports.
        /// </summary>
        /// <param name="networkMask">The network mask to scan in CIDR notation (e.g. "192.168.0.0/24" or ). If no mask is provided the ICMPScan.cs added a /32 to scan a single IP. </param>
        /// <param name="portRange">The range of ports to scan (e.g 1-100 or 80, 433, 22). If not provided default is 1-1024. </param>
        /// <returns>A boolean value of True if the scan completed successfully, otherwise false</returns>

        public async Task<bool> DoPortScan(string networkMask, string portRange)
        {
            var ports = PortUtility.ParsePortRange(portRange);

            ICMPScan hostScan = new ICMPScan(networkMask);
            bool success = await hostScan.DoScan();
            if (success)
            {
                List<Task<Host>> hostTasks = new List<Task<Host>>();
                foreach (Host host in hostScan.results)
                {
                    hostTasks.Add(ScanHost(host, ports));
                }
                var hosts = await Task.WhenAll(hostTasks);
                results.AddRange(hosts);
            }
            return true;
        }

        private async Task<Host> ScanHost(Host host, IEnumerable<int> ports)
        {
            ConcurrentBag<Port> openPorts = new ConcurrentBag<Port>();

            await Task.Run(() =>
            {
                Parallel.ForEach(ports, async portNum =>
                {
                    Port? port = await ScanPort(host, portNum);
                    if (port != null)
                    {
                        openPorts.Add(port);
                    }
                });
            });

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