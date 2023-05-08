using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;


namespace NetInspectLib.Networking
{
    /// <summary>
    /// Class for doing a WHOIS query
    /// <example>
    /// Usage
    /// <code>
    ///     Whois whois = new Whois();
    ///     string result = whois.Lookup("Google.com");
    ///     //Do something with result e.g Console.Writeline(result); 
    /// </code>
    /// </example>
    /// </summary>

    public class Whois
    {
        private const int DEFAULT_PORT = 43;
        private const string DEFAULT_SERVER = "whois.iana.org";

        /// <summary>
        /// Does a query for a specfied input using IANA WHOIS directory. 
        /// </summary>
        /// <param name="query">The query to preform (e.g Google.com). </param>
        /// <param name="server">The WHOIS server to get the results from. using "whois.iana.org"</param>
        /// <param name="port">The port to connect to the WHOIS server. </param>
        /// <returns>The results of the WHOIS query. </returns>
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
                            string newServer = line.Substring(6).Trim();
                            result += Lookup(query, newServer, port);
                            break;
                        }
                    }
                }
            }

            result = Regex.Replace(result, "^#.*", "", RegexOptions.Multiline);
            result = Regex.Replace(result, "^\\s*$\n", "", RegexOptions.Multiline);

            return result.Trim();
        }
    }
}

