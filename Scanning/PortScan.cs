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
    public class PortScan
    {
        public List<Host> results { get; }

        public PortScan()
        {
            results = new List<Host>();
        }

        public async Task<bool> DoPortScan(string networkMask, string portRange)
        {
            var ports = ParsePortRange(portRange);

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
            foreach (int portNum in ports)
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
                    tcpClient.Connect(host.IPAdress, portNum);
                    Debug.WriteLine($"Host: {host.IPAdress} Port {portNum} is OPEN");
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