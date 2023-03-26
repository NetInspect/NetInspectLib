using System.Collections;
using System.Net;

using NetInspectLib.Types;
using System.Net.NetworkInformation;
using System.Diagnostics;

namespace NetInspectLib.Networking
{
    public class Traceroute : IEnumerable
    {
        public Route route { get; }

        public Traceroute()
        {
            route = new Route();
        }

        public Task<bool> DoTraceroute(string hostname, int maxHops)
        {
            IPAddress[] addresses;

            try
            {
                addresses = Dns.GetHostAddresses(hostname);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting IP address for {hostname}: {ex.Message}");
                return Task.FromResult(false);
            }

            if (addresses.Length == 0)
            {
                Debug.WriteLine($"No IP address found for {hostname}");
                return Task.FromResult(false);
            }

            IPAddress ip = addresses[0];

            for (int ttl = 1; ttl <= maxHops; ttl++)
            {
                PingOptions options = new PingOptions(ttl, true);
                Ping ping = new Ping();

                try
                {
                    PingReply reply = ping.Send(ip, 5000, new byte[32], options);

                    if (reply.Status == IPStatus.TtlExpired || reply.Status == IPStatus.Success)
                    {
                        string address = reply.Address == null ? "*" : reply.Address.ToString();
                        string time = reply.Status == IPStatus.Success ? reply.RoundtripTime + " ms" : "timeout";

                        route.AddHop(address, time);

                        if (reply.Status == IPStatus.Success)
                        {
                            break;
                        }
                    }
                    else
                    {
                        route.AddHop("*", "timeout");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error pinging {ip}: {ex.Message}");
                    route.AddHop("*", "timeout");
                }
            }

            return Task.FromResult(true);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var hop in route.hops)
            {
                yield return hop;
            }
        }
    }
}