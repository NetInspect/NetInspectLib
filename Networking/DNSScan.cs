using System;
using System.Collections.Generic;
using System.Net;
using DnsClient;
using DnsClient.Protocol;

public class DnsRecord
{
    public string? DomainName { get; set; }
    public string? RecordClass { get; set; }
    public string? RecordType { get; set; }
    public string? TimeToLive { get; set; }
    public string? Data { get; set; }
}

/// <summary>
/// Provides methods to perform DNS lookups and retrieve DNS records.
/// /// <example>
/// Usage
/// <code>
///     DnsLookup dnsLookup = new DnsLookup();
///     List<DnsRecord> results = dnsLookup.DoDNSLookup("google.com", QueryType.ANY);
///     foreach(DnsRecord result in results)
///     {
///             //Do Something
///     }
/// </code>
/// </example>
/// </summary>
public class DnsLookup
{
    private readonly LookupClient _dnsClient;

    /// <summary>
    /// Initializes a new instance of the DnsLookup class with the specified DNS server.
    /// If no DNS server is specified, a default one is used.
    /// </summary>
    /// <param name="dnsServer">The IP address of the DNS server to use.</param>
    public DnsLookup(string? dnsServer = null)
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
    /// <summary>
    /// Looksup a domain or host name for DNS information with a given DNS server.
    /// </summary>
    /// <param name="domain_or_ip">The domain name or host IP for a reverse lookup (e.g "Google.com").</param>
    /// <param name="queryType">The DNS record to query (e.g "A" for A records) if left empty or null the default is a "ANY" query. Note some DNS servers don't except "ANY" queries.</param>
    /// <returns>A list of DNS records</returns>
    public List<DnsRecord> DoDNSLookup(string domain_or_ip, QueryType queryType = QueryType.ANY)
    {
        List<DnsRecord> results = new List<DnsRecord>();
        try
        {
            IDnsQueryResponse response = _dnsClient.Query(domain_or_ip, queryType);
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
                DomainName = domain_or_ip,
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
