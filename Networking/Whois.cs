using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

public class Whois
{
    private const int DEFAULT_PORT = 43;
    private const string DEFAULT_SERVER = "whois.iana.org";

    public string Lookup(string query, string server = DEFAULT_SERVER, int port = DEFAULT_PORT)
    {
        string result = "";

        using (TcpClient client = new TcpClient())
        {
            client.Connect(server, port);

            using (NetworkStream stream = client.GetStream())
            using (StreamWriter writer = new StreamWriter(stream, Encoding.ASCII))
            using (StreamReader reader = new StreamReader(stream, Encoding.ASCII))
            {
                writer.WriteLine(query);
                writer.Flush();

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    result += line + "\n";
                    if (line.StartsWith("refer:", System.StringComparison.OrdinalIgnoreCase))
                    {
                        // If the WHOIS server returns a "refer" response, follow the new server and query again
                        string newServer = line.Substring(6).Trim();
                        result += Lookup(query, newServer, port);
                        break;
                    }
                }
            }
        }

        // Remove comments and blank lines from the response
        result = Regex.Replace(result, "^#.*", "", RegexOptions.Multiline);
        result = Regex.Replace(result, "^\\s*$\n", "", RegexOptions.Multiline);

        return result.Trim();
    }
}
