using System.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Concurrent;

using NetInspectLib.Types;
using NetInspectLib.Networking.Utilities;

namespace NetInspectLib.Discovery
{
    public class ARPScan
    {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int DestIP, int SrcIP, byte[] pMacAddr, ref uint PhyAddrLen);

        private static uint macAddrLen = (uint)new byte[6].Length;

        public List<Host> results;

        public ARPScan()
        {
            results = new List<Host>();
        }

        private string[]? SendArpRequestAsync(IPAddress network, int cidr, int hostNum)
        {
            var ip = IPHelper.GetAddress(network, cidr, hostNum);
            byte[] macAddr = new byte[6];
            try
            {
                SendARP((int)BitConverter.ToInt32(ip.GetAddressBytes(), 0), 0, macAddr, ref macAddrLen);
                var macString = BitConverter.ToString(macAddr).ToUpper();
                if (macString != "00-00-00-00-00-00")
                {
                    return new string[] { ip.ToString(), macString };
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Task<bool> DoARPScan(string networkMask)
        {
            try
            {
                var cidr = int.Parse(networkMask.Split('/')[1]);
                var network = IPHelper.GetNetworkAddress(IPAddress.Parse(networkMask.Split('/')[0]), IPHelper.GetMask(cidr));

                ConcurrentBag<string[]> scanResults = new ConcurrentBag<string[]>();
                List<Thread> threads = new List<Thread>();

                for (int i = 1; i < (1 << (32 - cidr)) - 2; i++)
                {
                    Thread thread = new Thread(() =>
                    {
                        string[]? result = SendArpRequestAsync(network, cidr, i);
                        if (result != null) scanResults.Add(result);
                    });
                    threads.Add(thread);
                    thread.Start();
                }

                foreach (Thread thread in threads) { thread.Join(); }

                foreach (var result in scanResults.OrderBy(x => x, new IPv4Comparer()).ToList())
                {
                    if (!results.Any(ip => ip.IPAdress.ToString() == result[0]))
                    {
                        Host host = new Host(result[0], result[1]);
                        results.Add(host);
                    }
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ARP Scan Error: {ex.Message}");
                return Task.FromResult(false);
            }
        }
    }

    internal class IPv4Comparer : IComparer<string[]>
    {
        public int Compare(string[]? x, string[]? y)
        {
            if (x == null || y == null) return 0;
            var xInt = IPAddressToUInt32(x[0]);
            var yInt = IPAddressToUInt32(y[0]);
            return xInt.CompareTo(yInt);
        }

        private static uint IPAddressToUInt32(string ipAddress)
        {
            var bytes = ipAddress.Split('.')
                .Select(byte.Parse)
                .Reverse()
                .ToArray();
            return BitConverter.ToUInt32(bytes, 0);
        }
    }
}