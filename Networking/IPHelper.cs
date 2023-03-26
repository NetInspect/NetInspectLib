using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

namespace NetInspectLib.Networking
{
    internal static class IPHelper
    {
        [DllImport("iphlpapi.dll", SetLastError = true)]
        public static extern int GetAdaptersInfo(IntPtr pAdapterInfo, ref uint pOutBufLen);

        public const int ERROR_BUFFER_OVERFLOW = 111;
        public const int NO_ERROR = 0;

        public const int MAX_ADAPTER_NAME_LENGTH = 256;
        public const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128;
        public const int MAX_ADAPTER_ADDRESS_LENGTH = 8;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct IP_ADAPTER_INFO
        {
            public IntPtr Next;
            public uint ComboIndex;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_NAME_LENGTH + 4)]
            public string AdapterName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_DESCRIPTION_LENGTH + 4)]
            public string Description;

            public uint AddressLength;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ADAPTER_ADDRESS_LENGTH)]
            public byte[] Address;

            public uint Index;
            public uint Type;
            public uint DhcpEnabled;
            public IntPtr CurrentIpAddress;
            public IP_ADDR_STRING IpAddressList;
            public IP_ADDR_STRING GatewayList;
            public IP_ADDR_STRING DhcpServer;
            public bool HaveWins;
            public IP_ADDR_STRING PrimaryWinsServer;
            public IP_ADDR_STRING SecondaryWinsServer;
            public uint LeaseObtained;
            public uint LeaseExpires;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct IP_ADDR_STRING
        {
            public IntPtr Next;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string IpAddress;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string IpMask;

            public uint Context;
        }

        public static IPAddress GetNetworkAddress(IPAddress address, IPAddress subnetMask)
        {
            byte[] ipBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipBytes.Length != subnetMaskBytes.Length)
            {
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");
            }

            byte[] networkAddressBytes = new byte[ipBytes.Length];
            for (int i = 0; i < networkAddressBytes.Length; i++)
            {
                networkAddressBytes[i] = (byte)(ipBytes[i] & subnetMaskBytes[i]);
            }

            return new IPAddress(networkAddressBytes);
        }

        public static IPAddress GetMask(int cidr)
        {
            uint mask = 0xffffffff;
            mask <<= (32 - cidr);
            byte[] maskBytes = BitConverter.GetBytes(mask);
            Array.Reverse(maskBytes);
            return new IPAddress(maskBytes);
        }

        public static IPAddress GetAddress(IPAddress netAddr, int cidr, int host)
        {
            byte[] bytes = netAddr.GetAddressBytes();
            Array.Reverse(bytes);
            uint ip = BitConverter.ToUInt32(bytes, 0);
            uint subnetMask = ~(uint.MaxValue >> cidr);
            int maxHost = (int)Math.Pow(2, 32 - cidr) - 2;
            if (host < 0 || host > maxHost)
            {
                throw new ArgumentOutOfRangeException(nameof(host), $"Host number must be between 0 and {maxHost}");
            }
            ip &= subnetMask;
            ip <<= (IPAddress.NetworkToHostOrder((int)netAddr.AddressFamily) * 8);
            ip += (uint)host;
            byte[] ipBytes = BitConverter.GetBytes(IPAddress.NetworkToHostOrder((int)ip));
            return new IPAddress(ipBytes);
        }
    }
}