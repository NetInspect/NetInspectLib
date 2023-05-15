using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetInspectLib.Networking
{
    public struct DnsRecord
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Class { get; set; }
        public string TTL { get; set; }
        public string Data { get; set; }
    }

    public class DnsResolver
    {
        public enum DNSRecordType
        {
            A = 1,
            AAAA = 28,
            CNAME = 5,
            NS = 2,
            MX = 15
        }

        private IPAddress dnsServer;

        public DnsResolver(string dnsServer = "8.8.8.8")
        {
            this.dnsServer = IPAddress.Parse(dnsServer);
        }

        public List<DnsRecord> DoDNSLookup(string domain_or_ip)
        {
            List<DnsRecord> results = new List<DnsRecord>();

            foreach (var recordType in Enum.GetValues(typeof(DNSRecordType)).Cast<DNSRecordType>())
            {
                using (UdpClient udpClient = new UdpClient())
                {
                    udpClient.Connect(dnsServer, 53);
                    byte[] queryMessage = CreateDNSQuery(domain_or_ip, recordType);
                    udpClient.Send(queryMessage, queryMessage.Length);

                    IPEndPoint dnsServerEndPoint = new IPEndPoint(dnsServer, 0);
                    byte[] responseMessage = udpClient.Receive(ref dnsServerEndPoint);

                    DnsRecord dnsRecord = ParseDnsResponce(responseMessage);
                    dnsRecord.Type = recordType.ToString();
                    results.Add(dnsRecord);
                }
            }

            return results;
        }

        private DnsRecord ParseDnsResponce(byte[] response)
        {
            DnsRecord record = new DnsRecord();

            // The RCode is stored in the last 4 bits of the second byte of the DNS response
            byte rcode = (byte)(response[3] & 0x0F); // 0x0F = 00001111 in binary

            if (rcode == 0)
            {
                //I've spent too long on this, removed code, implment later
            }

            return record;
        }

        private byte[] CreateDNSQuery(string domain, DNSRecordType recordType)
        {
            ushort queryId = (ushort)new Random().Next(0xFFFF);

            byte[] queryBytes = new byte[12 + domain.Length + 2 + 4];

            // Query ID (2 bytes)
            queryBytes[0] = (byte)(queryId >> 8);
            queryBytes[1] = (byte)(queryId & 0xFF);

            // Flags (2 bytes)
            queryBytes[2] = 0x01;  // Standard query
            queryBytes[3] = 0x00;  // No flags set

            // Questions count (2 bytes)
            queryBytes[4] = 0x00;
            queryBytes[5] = 0x01;

            // Answers, Authorities, and Additional records count (each 2 bytes)
            queryBytes[6] = 0x00;
            queryBytes[7] = 0x00;
            queryBytes[8] = 0x00;
            queryBytes[9] = 0x00;
            queryBytes[10] = 0x00;
            queryBytes[11] = 0x00;

            // Domain name (variable length)
            string[] domainParts = domain.Split('.');
            int currentPosition = 12;

            foreach (string part in domainParts)
            {
                queryBytes[currentPosition] = (byte)part.Length;
                currentPosition++;

                foreach (char c in part)
                {
                    queryBytes[currentPosition] = (byte)c;
                    currentPosition++;
                }
            }

            // End of domain name (0x00)
            queryBytes[currentPosition] = 0x00;
            currentPosition++;

            // Query type (2 bytes)
            queryBytes[currentPosition] = (byte)((ushort)recordType >> 8);
            queryBytes[currentPosition + 1] = (byte)((ushort)recordType & 0xFF);

            // Query class (2 bytes) - IN (Internet)
            queryBytes[currentPosition + 2] = 0x00;
            queryBytes[currentPosition + 3] = 0x01;

            return queryBytes;
        }
    }
}