using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetInspectLib.Discovery;
using NetInspectLib.Types;

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
            var ports = ParsePortRange(portRange);

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
            List<Task> portTasks = new List<Task>();
            foreach (int portNum in ports)
            {
                portTasks.Add(Task.Run(async () =>
                {
                    Port? port = await ScanPort(host, portNum);
                    if (port != null)
                    {
                        openPorts.Add(port);
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
                tcpClient.ReceiveTimeout = 500;
                tcpClient.SendTimeout = 500;
                try
                {
                    await tcpClient.ConnectAsync(host.IPAddress, portNum);
                    tcpClient.Close();
                    return new Port(portNum);
                }
                catch (SocketException)
                {
                    tcpClient.Close();
                    return null;
                }
            }
        }

        private static IEnumerable<int> ParsePortRange(string portRange)
        {
            var ports = new List<int>();

            try
            {
                if (string.IsNullOrWhiteSpace(portRange))
                {
                    return Enumerable.Range(1, 1024);
                }

                foreach (var item in portRange.Split(','))
                {
                    var range = item.Trim().Split('-');

                    if (range.Length == 1)
                    {
                        if (int.TryParse(range[0], out var port))
                        {
                            ports.Add(port);
                        }
                    }
                    else if (range.Length == 2)
                    {
                        if (int.TryParse(range[0], out var start) && int.TryParse(range[1], out var end))
                        {
                            ports.AddRange(Enumerable.Range(start, end - start + 1));
                        }
                    }
                }

                return ports.Distinct();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing port range: {ex.Message}");
                return Enumerable.Empty<int>();
            }
        }
    }
}
