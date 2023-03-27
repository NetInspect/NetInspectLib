using NetInspectLib.Types;
using System.Collections.Generic;

public class DNS
{
    public Host? Host { get; set; }
    public string? RecordType { get; set; }
    public int TTL { get; set; }
    public List<string>? Data { get; set; }
    public string? CName { get; set; }
    public string? NameServer { get; set; }
}