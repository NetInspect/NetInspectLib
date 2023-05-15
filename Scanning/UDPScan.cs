using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NetInspectLib.Discovery;
using NetInspectLib.Networking;
using NetInspectLib.Types;

namespace NetInspectLib.Scanning
{
    public class UDPScan
    {
        public List<Host> results { get; }

        public UDPScan()
        {
            results = new List<Host>();
        }

        public async Task<bool> DoUDPScan(string networkMask)
        {
            ICMPScan hostScan = new ICMPScan(networkMask);
            Task<bool> scan = hostScan.DoScan();
            bool success = await scan;
            if (success)
            {
                foreach (Host host in hostScan.results)
                {
                    results.Add(ScanHost(host));
                }
            }
            return true;
        }

        private Host ScanHost(Host host)
        {
            List<Thread> threads = new List<Thread>();
            ConcurrentBag<Port> openPorts = new ConcurrentBag<Port>();
            for (int portNum = 1; portNum <= 1024; portNum++)
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
            using (var udpClient = new UdpClient())
            {
                udpClient.Client.ReceiveTimeout = 500;
                udpClient.Client.SendTimeout = 500;
                try
                {
                    Byte[] sendBytes = Encoding.ASCII.GetBytes("Is this port open");
                    udpClient.Send(sendBytes, sendBytes.Length, host.IPAddress.ToString(), portNum);

                    var remoteEP = new IPEndPoint(IPAddress.Any, portNum);
                    var response = udpClient.Receive(ref remoteEP);
                    Debug.WriteLine($"Host: {host.IPAddress} Port {portNum} is OPEN");
                    udpClient.Close();
                    return new Port(portNum);
                }
                catch (SocketException e)
                {
                    udpClient.Close();
                    return null;
                }
            }
        }
    }
}