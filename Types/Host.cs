using System.Net;

namespace NetInspectLib.Types
{
    public class Host
    {
        public string? Hostname { get; set; }
        public IPAddress IPAdress { get; set; }
        public string? MacAddress { get; set; }
        public List<Port> Ports { get; private set; } = new List<Port>();

        public Host(string ipAdress, string? hostname = null, string? macAddress = null)
        {
            Hostname = hostname;
            IPAdress = IPAddress.Parse(ipAdress);
            MacAddress = macAddress;
        }

        public Host(IPAddress ipAdress, string? hostname = null, string? macAddress = null)
        {
            Hostname = hostname;
            IPAdress = ipAdress;
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
}