namespace NetInspectLib.Types
{
    public class Port
    {
        public int Number { get; }
        public string Name { get; }

        private static readonly Dictionary<int, string> knownPorts = new Dictionary<int, string>()
        {
            { 20, "FTP Data" }, { 21, "FTP Control" },
            { 22, "SSH" },      { 23, "Telnet" },
            { 25, "SMTP" },     { 43, "whois" },
            { 53, "DNS" },      { 68, "DHCP" },
            { 80, "HTTP" },     { 110, "POP" },
            { 115, "SFTP" },      { 119, "NNTP" },
            { 123, "NTP" },     { 143, "IMAP" },
            { 161, "SNMP" },      { 220, "IMAP3" },
            { 389, "LDAP" },      { 443, "HTTPS" },
            { 445, "SMB" },   { 613, "IPP" },
            { 3306, "MySQL" },  {5351, "NAT-PMP"} 
          
        };

        public Port(int number, string? name = null)
        {
            Number = number;
            if (name == null && knownPorts.ContainsKey(number))
            {
                Name = knownPorts[number];
            }
            else
            {
                Name = name ?? "UNKNOWN";
            }
        }
    }
}
