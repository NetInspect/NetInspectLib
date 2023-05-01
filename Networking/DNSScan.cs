/* Uses https://github.com/MichaCo/DnsClient.NET */

using System;
using System.Collections.Generic;
using System.Net;
using DnsClient;
using DnsClient.Protocol;

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

    public List<string> DoDNSLookup(string hostOrIp, QueryType queryType = QueryType.ANY)
    {
        List<string> results = new List<string>();
        try
        {
            IDnsQueryResponse response = _dnsClient.Query(hostOrIp, queryType);
            results.Add($"Query type: {response.Questions[0].QuestionType}");

            foreach (DnsResourceRecord record in response.Answers)
            {
                results.Add($" {record.ToString()}");
                // {DomainName} {RecordClass} {RecordType} {TimeToLive} {Data}
            }
        }
        catch (DnsResponseException ex)
        {
            results.Add($"Query failed: {ex.Message}");
        }
        return results;
    }
}
