namespace NetInspectLib.Types
{
    public class Port
    {
        public int Number { get; }
        public string Name { get; }

        private Dictionary<int, string> knownPorts = new Dictionary<int, string>()
        {
            { 20, "FTP Data" }, { 21, "FTP Control" },
            { 22, "SSH" },      { 23, "Telnet" },
            { 25, "SMTP" },     { 53, "DNS" },
            { 80, "HTTP" },     { 110, "POP" },
            { 123, "NTP" },     { 143, "IMAP" },
            { 443, "HTTPS" },   { 613, "IPP" },
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
                Name = name ?? String.Empty;
            }
        }
    }
}