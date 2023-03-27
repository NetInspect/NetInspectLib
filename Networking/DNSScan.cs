using System.Net.Sockets;
using System.Net;
using NetInspectLib.Types;

public class DNSScanner
{
    public List<DNS> DoDNSScan(string target)
    {
        var records = new List<DNS>();

        // Lookup hostname and add A records
        var hostEntry = Dns.GetHostEntry(target);
        foreach (var address in hostEntry.AddressList)
        {
            records.Add(new DNS
            {
                //Hostname = target,
                //IPAddress = address.ToString(),
                Host = new Host(address.ToString(), target),
                RecordType = "A",
                TTL = (int)hostEntry.AddressList[0].AddressFamily,
                Data = null,
                CName = hostEntry.HostName,
                NameServer = Dns.GetHostEntry(target).HostName
            });
        }

        // Lookup other records (CNAME, MX, TXT)
        foreach (var recordType in new string[] { "CNAME", "MX", "TXT" })
        {
            try
            {
                var DNSs = Dns.GetHostEntry($"{target}.{recordType}");
                foreach (var DNS in DNSs.AddressList)
                {
                    records.Add(new DNS
                    {
                        //Hostname = target,
                        //IPAddress = DNS.ToString(),
                        Host = new Host(DNS.ToString(), target),
                        RecordType = recordType,
                        TTL = (int)DNSs.AddressList[0].AddressFamily,
                        Data = null,
                        CName = hostEntry.HostName,
                        NameServer = Dns.GetHostEntry(target).HostName
                    });
                }
            }
            catch (SocketException ex)
            {
                // Ignore exceptions caused by records that don't exist
                if (ex.SocketErrorCode != SocketError.HostNotFound && ex.SocketErrorCode != SocketError.NoData)
                {
                    continue; //throw
                }
            }
        }

        return records;
    }
}