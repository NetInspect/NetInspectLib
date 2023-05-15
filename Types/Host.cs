using NetInspectLib.Types;
using System.Net;
using System.Text.RegularExpressions;

public class Host
{
    private string? _hostname;
    public string? Hostname
    {
        get => _hostname;
        set
        {
            // do validation here on "value"
            if (value != null)
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

    private string? _macAddress;
    public string? MacAddress
    {
        get => _macAddress;
        set
        {
            if (value != null)
            {
                Regex r = new Regex("^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})|([0-9a-fA-F]{4}\\.[0-9a-fA-F]{4}\\.[0-9a-fA-F]{4})$");
                if (r.IsMatch(value)) _macAddress = value;
                else throw new ArgumentException($"{value} is not a vaild mac address");
            }
            else throw new ArgumentNullException("Mac Address cannot be null");

        }
    }

    private IPAddress? _ipAddress;
    public IPAddress? IPAddress
    {
        get => _ipAddress;
        set
        {
            // do validation here on "value"
            _ipAddress = value;
        }
    }

    public List<Port> Ports { get; private set; } = new List<Port>();

    public Host(string ipAddress, string? hostname = null, string? macAddress = null)
    {
        Hostname = hostname;
        IPAddress = IPAddress.Parse(ipAddress);
        MacAddress = macAddress;
    }

    public Host(IPAddress ipAddress, string? hostname = null, string? macAddress = null)
    {
        Hostname = hostname;
        IPAddress = ipAddress;
        MacAddress = macAddress;
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









