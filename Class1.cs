using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NetInspectLib.Networking;




using NetInspectLib.Discovery;

static async Task Main(string[] args)
{
    var arpScanner = new ARPScan();
    bool success = await arpScanner.DoARPScan("192.168.1.0/24");

    if (success)
    {
        foreach (var result in arpScanner.results)
        {
            Console.WriteLine($"IP: {result.Key}, MAC: {result.Value}");
        }
    }
}
