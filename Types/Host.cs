using System.Net;

namespace NetInspectLib.Types
{
    public class Host
    {
        private IPAddress ipAdress { get; set; }
        private string? macAddress { get; set; }
        private List<Port> ports { get; set; }

        public Host(string _ipAdress, string? _macAddress = null)
        {
            ipAdress = IPAddress.Parse(_ipAdress);
            macAddress = _macAddress;
            ports = new List<Port>();
        }

        public Host(IPAddress _ipAdress, string? _macAddress = null)
        {
            ipAdress = _ipAdress;
            macAddress = _macAddress;
            ports = new List<Port>();
        }

        public void AddPort(int portNumber, string? portName = null)
        {
            Port port = new Port(portNumber, portName);
            ports.Add(port);
        }

        public void AddPort(Port port)
        {
            ports.Add(port);
        }

        public IPAddress GetIPAddress()
        {
            return ipAdress;
        }

        public string? GetMacAddress()
        {
            return macAddress;
        }

        public void SetMacAddress(string _macAddress)
        {
            macAddress = _macAddress;
        }

        public List<Port> GetPorts()
        {
            return ports;
        }
    }
}