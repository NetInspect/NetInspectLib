using System.Net.Sockets;
using System.Net;
using NetInspectLib.Types;
using System.Collections.Generic;

public class DNSScanner
{
    public List<DNS> DoDNSScan(string target)
    {
        var records = new List<DNS>();
        var hostEntry = Dns.GetHostEntry(target);

        foreach (var address in hostEntry.AddressList)
        {
            records.Add(new DNS
            {
                Host = new Host(address.ToString(), target),
                RecordType = "A",
                Data = null,
                CName = hostEntry.HostName,
                NameServer = Dns.GetHostEntry(target).HostName
            });

        }

        foreach (var recordType in new string[] { "CNAME", "MX", "TXT", "AAAA", "NS", "SOA", "PTR", "DNSKEY", "DS", "NAPTR" })
        {
            try
            {
                var DNSs = Dns.GetHostEntry($"{target}.{recordType}");

                foreach (var DNS in DNSs.AddressList)
                {
                    records.Add(new DNS
                    {
                        Host = new Host(DNS.ToString(), target),
                        RecordType = recordType,
                        Data = null,
                        CName = hostEntry.HostName,
                        NameServer = Dns.GetHostEntry(target).HostName
                    });
                }
            }

            catch (SocketException ex)
            {
                if (ex.SocketErrorCode != SocketError.HostNotFound && ex.SocketErrorCode != SocketError.NoData)
                {
                    continue;
                }
            }
        }

        return records;
    }
}