using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NetInspectLib.Networking.Utilities
{
    internal static class PortUtility
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct MIB_TCPROW_OWNER_PID
        {
            public uint dwState;
            public uint dwLocalAddr;
            public uint dwLocalPort;
            public uint dwRemoteAddr;
            public uint dwRemotePort;
            public uint dwOwningPid;
        }

        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, out int pdwSize, bool bOrder, int ulAf, int tableClass, int reserved);

        public static int GetProcessIdByPort(int port)
        {
            // Get the size of the TCP table
            int bufferSize = 0;
            uint result = GetExtendedTcpTable(IntPtr.Zero, out bufferSize, true, 2 /* AF_INET */, 5 /* TCP_TABLE_OWNER_PID_ALL */, 0);
            if (result != 0x00000000 && result != 0x7A) // ERROR_SUCCESS and ERROR_INSUFFICIENT_BUFFER
                throw new Exception($"GetExtendedTcpTable failed with error code {result}");

            // Allocate memory for the TCP table
            IntPtr tcpTablePtr = Marshal.AllocHGlobal(bufferSize);
            try
            {
                // Get the TCP table
                result = GetExtendedTcpTable(tcpTablePtr, out bufferSize, true, 2 /* AF_INET */, 5 /* TCP_TABLE_OWNER_PID_ALL */, 0);
                if (result != 0x00000000) // ERROR_SUCCESS
                    throw new Exception($"GetExtendedTcpTable failed with error code {result}");

                // Iterate over the TCP rows to find the one that matches the specified port
                MIB_TCPROW_OWNER_PID tcpRow;
                for (int i = 0; i < bufferSize / Marshal.SizeOf(typeof(MIB_TCPROW_OWNER_PID)); i++)
                {
                    tcpRow = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(tcpTablePtr + i * Marshal.SizeOf(typeof(MIB_TCPROW_OWNER_PID)));
                    if (IPAddress.NetworkToHostOrder(tcpRow.dwLocalPort) == port)
                        return (int)tcpRow.dwOwningPid;
                }

                // If no matching row was found, return -1
                return -1;
            }
            finally
            {
                Marshal.FreeHGlobal(tcpTablePtr);
            }
        }
    }
}