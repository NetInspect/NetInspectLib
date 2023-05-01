using System.Net.Sockets;
using System.IO;
using System.Text;

public class Whois
{
    private const int DEFAULT_PORT = 43;
    private const string DEFAULT_SERVER = "whois.iana.org";

    public string DoWhoIsLookup(string query, string server = DEFAULT_SERVER, int port = DEFAULT_PORT)
    {
        string result = "";
        using (TcpClient client = new TcpClient(server, port))
        {
            using (NetworkStream stream = client.GetStream())
            {
                byte[] queryBytes = Encoding.ASCII.GetBytes(query + "\r\n");
                stream.Write(queryBytes, 0, queryBytes.Length);

                using (StreamReader reader = new StreamReader(stream, Encoding.ASCII))
                {
                    result = reader.ReadToEnd();
                }
            }
        }

        return result;
    }
}
