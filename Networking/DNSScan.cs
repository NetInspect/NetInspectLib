using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetInspectLib.Networking
{
    /// <summary>
    /// Provides methods to perform DNS lookups and retrieve DNS records.
    /// </summary>
    public class DnsLookup
    {
        /// <summary>
        /// Looksup a domain or host name for DNS information.
        /// </summary>
        /// <param name="domainName">The domain name or host IP for a reverse lookup (e.g., "Google.com").</param>
        /// <param name="queryType">The DNS record to query (e.g., "A" for A records). Note that some DNS servers don't accept "ANY" queries, as why Google was choosen over Cloudflare</param>
        /// <returns>A list of DNS records.</returns>
        public async Task<List<DnsRecord>> DoDNSLookup(string domainName, string queryType)
        {

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://dns.google/resolve?name={domainName}&type={queryType}&do=true");
            request.Headers.Add("Accept", "application/dns-json");

            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            DnsResponse dnsResponse = JsonSerializer.Deserialize<DnsResponse>(json);

            List<DnsRecord> dnsRecords = new List<DnsRecord>();

            if (dnsResponse?.Answer != null)
            {
                foreach (var answer in dnsResponse.Answer)
                {
                    dnsRecords.Add(new DnsRecord
                    {
                        Name = answer.A_Name,
                        RecordType = ConvertRecordType(answer.A_Type),
                        TTL = answer.A_Ttl,
                        Data = answer.A_Data,
                    });
                }
            }

            return dnsRecords;
        }

        private string ConvertRecordType(int type)
        {
            switch (type)
            {
                case 1:
                    return "A";
                case 2:
                    return "NS";
                case 5:
                    return "CNAME";
                case 6:
                    return "SOA";
                case 12:
                    return "PTR";
                case 15:
                    return "MX";
                case 16:
                    return "TXT";
                case 28:
                    return "AAAA";
                case 41:
                    return "OPT";
                case 257:
                    return "CAA";
                case 255:
                    return "ANY";
                default:
                    return "Unknown";
            }
        }

        private class DnsResponse
        {
            [JsonPropertyName("Status")]
            public int Status { get; init; }

            [JsonPropertyName("TC")]
            public bool Truncated { get; init; }

            [JsonPropertyName("RD")]
            public bool RecursiveDesired { get; init; }

            [JsonPropertyName("RA")]
            public bool RecursionAvailable { get; init; }

            [JsonPropertyName("AD")]
            public bool DnssecVerified { get; init; }

            [JsonPropertyName("Question")]
            public List<DnsQuestion> Question { get; init; }

            [JsonPropertyName("Answer")]
            public List<DnsAnswer> Answer { get; init; }

            [JsonPropertyName("Authority")]
            public List<DnsAnswer> Authority { get; init; }

            [JsonPropertyName("Additional")]
            public List<DnsAnswer> Additional { get; init; }

            public class DnsQuestion
            {
                [JsonPropertyName("name")]
                public string Q_Name { get; init; }

                [JsonPropertyName("type")]
                public int Q_Type { get; init; }
            }

            public class DnsAnswer
            {
                [JsonPropertyName("name")]
                public string A_Name { get; init; }

                [JsonPropertyName("type")]
                public int A_Type { get; init; }

                [JsonPropertyName("TTL")]
                public int A_Ttl { get; init; }

                [JsonPropertyName("data")]
                public string A_Data { get; init; }
            }
        }

       
        public class DnsRecord
        {

            public string Name { get; set; }
  
            public string RecordType { get; set; }

            public int TTL { get; set; }

            public string Data { get; set; }

        }

    }
}
