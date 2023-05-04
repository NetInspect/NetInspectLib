using System.Collections.Concurrent;
using System.Net.NetworkInformation;
using System.Net;
using System.Diagnostics;
using NetInspectLib.Types;
using NetInspectLib.Networking.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.ObjectModel;



namespace NetInspectLib.Discovery
{
    /// <summary>
    /// Class for ICMP Scanning of a network
    /// <example>
    /// Usage
    /// <code>
    ///     ICMPScan scanner = new ICMPScan();
    ///     Task<bool> scan = scanner.DoICMPScan("192.168.1.1/24");
    ///     bool success = await scan;
    ///     if(success)
    ///     {
    ///         foreach(host in scanner.results)
    ///         {
    ///             //Do Something
    ///         }
    ///     }
    /// </code>
    /// </example>
    /// </summary>
    public class ICMPScan
    {
        private string networkMask;
        public List<Host> results;

        public ICMPScan(string networkMask)
        {
            this.networkMask = networkMask;
            results = new List<Host>();
        }

        public ICMPScan(IPAddress ipAddress)
        {
            this.networkMask = ipAddress.ToString() + "/32";
            results = new List<Host>();
        }

        public Task<bool> DoScan()
        {
            try
            {
                if (!networkMask.Contains("/"))
                {
                    networkMask += "/32";
                }

                var cidr = int.Parse(networkMask.Split('/')[1]);
                var network = IPHelper.GetNetworkAddress(IPAddress.Parse(networkMask.Split('/')[0]), IPHelper.GetMask(cidr));

                var activeHosts = new ConcurrentBag<Host>();
                List<Thread> threads = new List<Thread>();

                if (cidr == 32)
                {
                    Host? result = SendPingRequest(IPAddress.Parse(networkMask.Split('/')[0]));
                    if (result != null) activeHosts.Add(result);
                }
                else
                {
                    for (int hostNum = 1; hostNum < (1 << (32 - cidr)) - 2; hostNum++)
                    {
                        Thread thread = new Thread(() =>
                        {
                            Host? result = SendPingRequest(network, cidr, hostNum);
                            if (result != null) activeHosts.Add(result);
                        });
                        threads.Add(thread);
                        thread.Start();
                    }

                    foreach (Thread thread in threads) { thread.Join(); }
                }

                results = activeHosts.OrderBy(host => host.GetIPAddress()).DistinctBy(host => host.GetIPAddress()).ToList();

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[-] DoPingSweep Error: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        private Host? SendPingRequest(IPAddress network, int cidr, int hostNum)
        {
            var ip = IPHelper.GetAddress(network, cidr, hostNum);

            Ping ping = new Ping();
            var pingResult = ping.Send(ip);
            if (pingResult.Status == IPStatus.Success)
            {
                return new Host(ip);
            }
            return null;
        }

        private Host? SendPingRequest(IPAddress ipAddress)
        {
            Ping ping = new Ping();
            var pingResult = ping.Send(ipAddress);
            if (pingResult.Status == IPStatus.Success)
            {
                return new Host(ipAddress);
            }
            return null;
        }
    }
}
