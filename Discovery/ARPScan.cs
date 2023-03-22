using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;

using NetInspectLib.Networking;
using System.Diagnostics;

namespace NetInspectLib.Discovery
{
    public class ARPScan
    {
        public Dictionary<string, List<string>> results;

        public ARPScan()
        {
            results = new Dictionary<string, List<string>>(); //ip: [mac, info]
        }

        private async Task<string[]> SendArpRequestAsync(IPAddress network, int cidr, byte[] macAddr, int hostNum)
        {
            var ip = IPHelper.GetAddress(network, cidr, hostNum);
            using (var client = new UdpClient())
            {
                try
                {
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    client.ExclusiveAddressUse = false;
                    client.Client.Bind(new IPEndPoint(IPAddress.Any, 0));

                    byte[] request = new byte[28];
                    byte[] response = new byte[28];

                    Buffer.BlockCopy(macAddr, 0, request, 0, 6);
                    Buffer.BlockCopy(macAddr, 0, request, 6, 6);
                    request[12] = 8;
                    request[13] = 6;
                    request[14] = 0;
                    request[15] = 1;
                    Buffer.BlockCopy(macAddr, 0, request, 16, 6);
                    Buffer.BlockCopy(ip.GetAddressBytes(), 0, request, 22, 4);

                    client.Send(request, request.Length, new IPEndPoint(ip, 0));

                    var task = Task.Run(() => client.ReceiveAsync());

                    if (await Task.WhenAny(task, Task.Delay(1000)) == task)
                    {
                        var result = task.Result;
                        if (result.Buffer.Length == 28 && result.Buffer.Skip(20).Take(4).SequenceEqual(ip.GetAddressBytes()))
                        {
                            var mac = string.Join(":", result.Buffer.Skip(6).Take(6).Select(b => b.ToString("X2")));
                            var adapterInfo = result.Buffer.Skip(22).Take(2).ToArray();
                            Debug.WriteLine($"Found {ip.ToString()}");
                            return new string[] { ip.ToString(), mac, BitConverter.ToString(adapterInfo) };
                        }
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public async Task<bool> DoARPScan(string networkMask)
        {
            try
            {
                var networkAddress = IPAddress.Parse(networkMask.Split('/')[0]);
                var cidr = int.Parse(networkMask.Split('/')[1]);
                var subnetMask = IPHelper.GetMask(cidr);
                var network = IPHelper.GetNetworkAddress(networkAddress, subnetMask);

                byte[] macAddr = new byte[6];
                uint adapterInfoLength = (uint)Marshal.SizeOf<IPHelper.IP_ADAPTER_INFO>();
                IntPtr pAdapterInfo = Marshal.AllocHGlobal((int)adapterInfoLength);

                // Get the local system MAC address
                if (IPHelper.GetAdaptersInfo(pAdapterInfo, ref adapterInfoLength) == IPHelper.ERROR_BUFFER_OVERFLOW)
                {
                    Marshal.FreeHGlobal(pAdapterInfo);
                    pAdapterInfo = Marshal.AllocHGlobal((int)adapterInfoLength);
                }

                if (IPHelper.GetAdaptersInfo(pAdapterInfo, ref adapterInfoLength) == IPHelper.NO_ERROR)
                {
                    var adapterInfo = (IPHelper.IP_ADAPTER_INFO)Marshal.PtrToStructure(pAdapterInfo, typeof(IPHelper.IP_ADAPTER_INFO));
                    macAddr = adapterInfo.Address.Take(6).ToArray();
                }

                Marshal.FreeHGlobal(pAdapterInfo);

                var arpTasks = Enumerable.Range(1, (1 << (32 - cidr)) - 2)
                    .Select(i => SendArpRequestAsync(network, cidr, macAddr, i));

                var arpResults = await Task.WhenAll(arpTasks);
                foreach (var arpResult in arpResults)
                {
                    if (arpResult != null)
                    {
                        Debug.WriteLine(arpResult.ToString());
                        //Add to "results" here
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ARP Scan Error: {ex.Message}");
                return false;
            }
        }
    }
}