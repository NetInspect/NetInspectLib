using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;



public class PortScan
{
    // Method that scans a range of IP addresses for open ports
    public static async Task<List<(string, int)>> ScanAsync(string input)
    {
        // Resolve the addresses to be scanned from the input string
        var addresses = await ResolveAddresses(input);

        // Initialize a list to store the results
        var results = new ConcurrentBag<(string, int)>();

        // Using well-known port range
        var ports = Enumerable.Range(1, 1024);

        // Loop over each address and scan the ports in parallel
        Parallel.ForEach(addresses, address =>
        {
            var openPorts = new List<int>();

            Parallel.ForEach(ports, port =>
            {
                if (IsPortOpenAsync(address, port).Result)
                {
                    openPorts.Add(port);
                }
            });

            // Add the open ports to the results list
            foreach (var openPortNumber in openPorts)
            {
                results.Add((address, openPortNumber));
            }
        });

        // Return the list of results
        return results.ToList();
    }





    private static async Task<List<string>> ResolveAddresses(string input)
    {
        var addressList = new List<string>();

        // If the input is a single IP address, return it as a single-item list
        if (IPAddress.TryParse(input, out var address))
        {
            addressList.Add(input);
        }
        // If the input is a range of IP addresses with a CIDR mask e.g 192.168.1.0/24
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

    // Helper method that pings an IP address to check if it is reachable
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


    // Method that checks if a port is open on a given IP address
    private static async Task<bool> IsPortOpenAsync(string address, int port)
    {
        try
        {
            // Try to connect to the IP address and port with a 3-second timeout
            using var client = new TcpClient();
            var task = client.ConnectAsync(address, port);
            var timeoutTask = Task.Delay(3000); //Timeout on the port if it takes to long
            await Task.WhenAny(task, timeoutTask);

            // If the connection is successful, the port is open
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                return true;
            }
        }
        catch (SocketException ex)
        {
            if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                return false;
            return true;
        }

        // If the connection attempt times out or fails, the port is not open
        return false;
    }
}