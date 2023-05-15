using NetInspectLib.Types;
using System.Net;
using System.Text.RegularExpressions;

public class Host
{
    private string _hostname = string.Empty;

    public string Hostname
    {
        get => _hostname;
        set
        {
            if (value != null || value != string.Empty)
            {
                if (value.Contains("\\")) throw new ArgumentException($"Hostname contains invailed character: \\");
                if (value.Contains("/")) throw new ArgumentException($"Hostname contains invailed character: /");
                if (value.Contains(":")) throw new ArgumentException($"Hostname contains invailed character: :");
                if (value.Contains("*")) throw new ArgumentException($"Hostname contains invailed character: *");
                if (value.Contains("?")) throw new ArgumentException($"Hostname contains invailed character: ?");
                if (value.Contains("\"")) throw new ArgumentException($"Hostname contains invailed character: \"");
                if (value.Contains("<")) throw new ArgumentException($"Hostname contains invailed character: <");
                if (value.Contains(">")) throw new ArgumentException($"Hostname contains invailed character: >");
                if (value.Contains("|")) throw new ArgumentException($"Hostname contains invailed character: |");
                _hostname = value;
            }
            else throw new ArgumentNullException("Hostname cannot be null");
        }
    }

    private string _macAddress = string.Empty;

    public string MacAddress
    {
        get => _macAddress;
        set
        {
            if (value != null && value != string.Empty)
            {
                Regex r = new Regex("^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})|([0-9a-fA-F]{4}\\.[0-9a-fA-F]{4}\\.[0-9a-fA-F]{4})$");
                if (r.IsMatch(value)) _macAddress = value;
                else throw new ArgumentException($"{value} is not a vaild mac address");
            }
            else throw new ArgumentNullException("Mac Address cannot be null");
        }
    }

    private IPAddress _ipAddress = IPAddress.None;

    public IPAddress IPAddress
    {
        get => _ipAddress;
        set
        {
            if (value != null)
            {
                _ipAddress = value;
            }
            else throw new ArgumentNullException("IP Address cannot be null");
        }
    }

    public List<Port> Ports { get; private set; } = new List<Port>();

    public Host(string ipAddress, string? hostname = null, string? macAddress = null)
    {
        IPAddress = IPAddress.Parse(ipAddress);
        if (hostname != null) Hostname = hostname;
        if (macAddress != null) MacAddress = macAddress;
    }

    public Host(IPAddress ipAddress, string? hostname = null, string? macAddress = null)
    {
        IPAddress = ipAddress;

        if (hostname != null) Hostname = hostname;
        if (macAddress != null) MacAddress = macAddress;
    }

    public void AddPort(int portNumber, PortStatus portStatus, string? portName = null)
    {
        Port port = new(portNumber, portStatus, portName);
        Ports.Add(port);
    }

    public void AddPort(Port port)
    {
        Ports.Add(port);
    }
}