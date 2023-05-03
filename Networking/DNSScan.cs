/* Uses https://github.com/MichaCo/DnsClient.NET */

using System;
using System.Collections.Generic;
using System.Net;
using DnsClient;
using DnsClient.Protocol;

public class DnsRecord
{
    public string DomainName { get; set; }
    public string RecordClass { get; set; }
    public string RecordType { get; set; }
    public string TimeToLive { get; set; }
    public string Data { get; set; }
}

public class DnsLookup
{
    private readonly LookupClient _dnsClient;

    public DnsLookup(string dnsServer = null)
    {
        if (string.IsNullOrEmpty(dnsServer))
        {
            _dnsClient = new LookupClient();
        }
        else
        {
            _dnsClient = new LookupClient(IPAddress.Parse(dnsServer));
        }
    }

    public List<DnsRecord> DoDNSLookup(string hostOrIp, QueryType queryType = QueryType.ANY)
    {
        List<DnsRecord> results = new List<DnsRecord>();
        try
        {
            IDnsQueryResponse response = _dnsClient.Query(hostOrIp, queryType);
            string queryTypeString = response.Questions[0].QuestionType.ToString();

            foreach (DnsResourceRecord record in response.Answers)
            {
                string[] parts = record.ToString().Split(' ');
                string data = parts[parts.Length - 1];
                DnsRecord dnsRecord = new DnsRecord
                {
                    DomainName = record.DomainName,
                    RecordClass = record.RecordClass.ToString(),
                    RecordType = record.RecordType.ToString(),
                    TimeToLive = record.TimeToLive.ToString(),
                    Data = data
                };
                results.Add(dnsRecord);
            }
        }
        catch (DnsResponseException ex)
        {
            DnsRecord dnsRecord = new DnsRecord
            {
                DomainName = hostOrIp,
                RecordClass = "Query failed",
                RecordType = ex.Message,
                TimeToLive = TimeSpan.Zero.ToString(),
                Data = ""
            };
            results.Add(dnsRecord);
        }
        return results;
    }
}
