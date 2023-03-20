using System.Collections.Concurrent;
using System.Net.NetworkInformation;
using System.Net;
using System.Diagnostics;

namespace NetInspectLib.Discovery
{
    public class ICMPScan
    {
        private ConcurrentBag<string> activeIPs;
        public List<string> results;

        public ICMPScan(bool _verbose = false)
        {
            activeIPs = new ConcurrentBag<string>();
            results = new List<string>();
        }

        private (IPAddress ipAddress, int subnetMask) ValidateNetworkMask(string networkMask)
        {
            try
            {
                var parts = networkMask.Split('/');
                if (parts.Length != 2)
                {
                    throw new ArgumentException("Invalid network mask format. Expected format: xxx.xxx.xxx.xxx/xx");
                }

                if (!IPAddress.TryParse(parts[0], out var ipAddress))
                {
                    throw new ArgumentException("Invalid IP address");
                }

                if (!int.TryParse(parts[1], out var subnetMask) || subnetMask < 0 || subnetMask > 32)
                {
                    throw new ArgumentException("Invalid subnet mask. Must be a number between 0 and 32");
                }

                return (ipAddress, subnetMask);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[-] ValidateNetworkMask Error: {ex.Message}");
                throw;
            }
        }

        public async Task<int> DoICMPScan(string networkMask)
        {
            try
            {
                var (ipAddress, subnetMask) = ValidateNetworkMask(networkMask);

                int numberOfHosts = (int)Math.Pow(2, 32 - subnetMask) - 2;
                if (subnetMask == 32)
                {
                    numberOfHosts = 1;
                }

                byte[] subnetMaskBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(-1 << 32 - subnetMask));

                var pingTasks = Enumerable.Range(0, numberOfHosts)
                    .Select(i => PingHostAsync(ipAddress, subnetMaskBytes, i));

                var pingResults = await Task.WhenAll(pingTasks);

                var activeIPs = AddSuccessfulPingResults(pingResults);

                var results = SortAndConvertActiveIPs(activeIPs);
                PrintResults(results);

                return 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[-] DoPingSweep Error: {ex.Message}");
                return 1;
            }
        }

        private async Task<PingReply> PingHostAsync(IPAddress ipAddress, byte[] subnetMaskBytes, int i)
        {
            byte[] nextIPAddressBytes = new byte[4];

            for (int j = 0; j < 4; j++)
            {
                nextIPAddressBytes[j] = (byte)(ipAddress.GetAddressBytes()[j] & subnetMaskBytes[j]);
            }

            nextIPAddressBytes[3] += (byte)i;
            IPAddress nextIPAddress = new IPAddress(nextIPAddressBytes);

            Ping ping = new Ping();
            return await ping.SendPingAsync(nextIPAddress);
        }

        private ConcurrentBag<string> AddSuccessfulPingResults(PingReply[] pingResults)
        {
            var activeIPs = new ConcurrentBag<string>();

            foreach (var pingReply in pingResults.Where(r => r.Status == IPStatus.Success))
            {
                activeIPs.Add(pingReply.Address.ToString());
                if (verbose) Console.Write($"\r[*] Found {activeIPs.Count} Hosts");
            }

            return activeIPs;
        }

        private List<string> SortAndConvertActiveIPs(ConcurrentBag<string> activeIPs)
        {
            return activeIPs
                .Select(Version.Parse)
                .OrderBy(arg => arg)
                .Select(arg => arg.ToString())
                .ToList();
        }

        private void PrintResults(List<string> results)
        {
            if (verbose) Console.WriteLine("\r\n==============================");
            foreach (var host in results)
            {
                if (verbose) Console.WriteLine($"[+] {host}");
            }
        }
    }
}