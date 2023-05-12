using NetInspectLib.Types;
using System.Net;

public class Host
{
    private string? _hostname;
    public string? Hostname
    {
        get => _hostname;
        set
        {
            // do validation here on "value"
            _hostname = value;
        }
    }

    private string? _macAddress;
    public string? MacAddress
    {
        get => _macAddress;
        set
        {
            // do validation here on "value"
            _macAddress = value;
        }
    }

    private IPAddress _ipAddress;
    public IPAddress IPAddress
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

    public void AddPort(int portNumber, string? portName = null)
    {
        Port port = new(portNumber, portName);
        Ports.Add(port);
    }

    public void AddPort(Port port)
    {
        Ports.Add(port);
    }
}









