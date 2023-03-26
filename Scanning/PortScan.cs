using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

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

        public async Task<bool> DoPortScan(string ipAddress, string portRange)
        {
            try
            {
                ThreadPool.SetMaxThreads(500, 500);

                var addresses = await ResolveAddresses(ipAddress);
                var ports = ParsePortRange(portRange);

                Parallel.ForEach(addresses, address =>
                {
                    Host host = new Host(address);

                    Parallel.ForEach(ports, async port =>
                    {
                        if (await CheckPortStatus(address, port, 3000))
                        {
                            host.AddPort(port);
                        }
                    });

                    results.Add(host);
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        private static async Task<List<string>> ResolveAddresses(string input)
        {
            var addressList = new List<string>();

            if (IPAddress.TryParse(input, out var address))
            {
                addressList.Add(input);
            }
            else if (IPAddress.TryParse(input.Split('/')[0], out var networkAddress))
            {
                if (!int.TryParse(input.Split('/')[1], out var cidrMask))
                {
                    Console.WriteLine($"Invalid subnet mask: {input}");
                    return addressList;
                }

                var subnetMask = ~0 << (32 - cidrMask);
                var startAddress = BitConverter.ToInt32(networkAddress.GetAddressBytes(), 0) & subnetMask;
                var endAddress = startAddress + ~subnetMask;
                var addressBytes = BitConverter.GetBytes(startAddress);

                while (BitConverter.ToInt32(addressBytes, 0) <= endAddress)
                {
                    var ipAddress = new IPAddress(addressBytes).ToString();
                    if (await PingIPAddress(ipAddress))
                    {
                        addressList.Add(ipAddress);
                    }
                    addressBytes = BitConverter.GetBytes(BitConverter.ToInt32(addressBytes, 0) + 1);
                }
            }
            else
            {
                Console.WriteLine($"Invalid IP address or subnet mask: {input}");
            }

            return addressList;
        }

        private static IEnumerable<int> ParsePortRange(string portRange)
        {
            var ports = new List<int>();

            try
            {
                if (string.IsNullOrWhiteSpace(portRange))
                {
                    return ports;
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

        private static async Task<bool> CheckPortStatus(string address, int port, int timeoutMilliseconds)
        {
            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var connectTask = socket.ConnectAsync(address, port);
                var timeoutTask = Task.Delay(timeoutMilliseconds);
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (completedTask == connectTask)
                {
                    return true;
                }
                socket.Close();
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking port {port} on {address}: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> PingIPAddress(string ipAddress)
        {
            using var ping = new Ping();
            try
            {
                var reply = await ping.SendPingAsync(ipAddress, 1000);
                return reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                return false;
            }
        }
    }
}