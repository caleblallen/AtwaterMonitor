using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;

namespace AtwaterMonitor
{
    //Code Adapted from https://msdn.microsoft.com/en-us/magazine/dn605876.aspx
    public class AsyncService
    {
        private IPAddress ipAddress;
        private int port;

        public AsyncService(int port)
        {
            // set up port and determine IP Address
            this.port = port;
            string hostName = Dns.GetHostName();
            IPHostEntry ipHostInfo = Dns.GetHostEntry(hostName);

            //Bind whatever IPAddresses are assigned to this server.
            for (int i = 0; i < ipHostInfo.AddressList.Length; ++i) {
                if (ipHostInfo.AddressList[i].AddressFamily == AddressFamily.InterNetwork &&
                    !ipHostInfo.AddressList[i].ToString().Contains("169.254"))
                {
                    this.ipAddress = ipHostInfo.AddressList[i];
                    break;
                }
            }

            if (this.ipAddress == null)
                throw new Exception("No IPv4 address for server");

            Console.WriteLine("Server bound to {0}", this.ipAddress.ToString());
        }

        public async void Run(ServiceCallback callback)
        {
            //Listen for TCP requests on the ipAddress and port ascertained earlier.
            TcpListener listener = new TcpListener(this.ipAddress, this.port);
            listener.Start();

            while (true)
            {
                try
                {
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                    Task t = Process(tcpClient,callback);
                    await t; // TODO: Combine with Line Above?
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        } 

        //Process an incomming request.
        private async Task Process(TcpClient tcpClient, ServiceCallback callback)
        {
            //Print the client connection to the console.
            //TODO: Remove this for version 1.
            string clientEndPoint = tcpClient.Client.RemoteEndPoint.ToString();
            Console.WriteLine("Received connection request from " + clientEndPoint);

            try
            {
                //Create and configure a stream we can use to send and receive data.
                NetworkStream networkStream = tcpClient.GetStream();
                StreamReader reader = new StreamReader(networkStream);
                StreamWriter writer = new StreamWriter(networkStream);
                writer.AutoFlush = true;


                //Stealing Liberally from https://stackoverflow.com/questions/26058594/how-to-get-all-data-from-networkstream
                //If we can read information from the networkStream...read it into a StringBuilder
                if(networkStream.CanRead)
                {
                    byte[] readBuffer = new byte[1024];

                    //Full webrequest.
                    StringBuilder fullMessage = new StringBuilder();

                    int numberOfBytesRead = 0;

                    //Read all data from the stream into fullMessage
                    do
                    {
                        numberOfBytesRead = await networkStream.ReadAsync(readBuffer,0,readBuffer.Length);

                        fullMessage.AppendFormat("{0}", Encoding.ASCII.GetString(readBuffer,0,numberOfBytesRead));

                    } while (networkStream.DataAvailable);


                    //Examine the request to see if it is properly formatted.
                    Tuple<String,String> opRequest = IsProperlyFormatted(fullMessage.ToString());

                    if (opRequest != null)
                    {
                        //Match the sent web request type with our Enum WebRequestType.
                        WebRequestType operation;
                        Enum.TryParse<WebRequestType>(opRequest.Item1, out operation);

                        //Invoke the callback that corresponds to Controller.HttpRequestHandler();
                        string dataFromController = callback.Invoke(operation,opRequest.Item2);

                        //Builder to prepare our response.
                        StringBuilder res = new StringBuilder("HTTP/1.1 200 OK\r\n");

                        //TODO: restrict cross origin to webserver only.
                        res.Append("Access-Control-Allow-Origin: *\r\n"); 
                        res.Append("Content-Type: text/*\r\n");
                        res.Append("\r\n");
                        res.Append(dataFromController);
                        await writer.WriteLineAsync(res.ToString());
                    }

                }
                else
                {
                    Console.WriteLine("Network Stream Unreadable.");
                }

                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (tcpClient.Connected)
                    tcpClient.Close();
            }
        } // Process

        private static Tuple<String,String> IsProperlyFormatted(string webRequest)
        {

            //Format a regular expression that filters out any non-supported WebRequestType's
            string regexPattern = String.Format("{0}",String.Join("|",Enum.GetNames(typeof(WebRequestType))));
            regexPattern = @"WebRequestType=(" + regexPattern + @")";
            Regex properWebRequestFormat = new Regex(regexPattern,RegexOptions.Compiled | RegexOptions.IgnoreCase);

            //Examine the web request for a supported WebRequestType
            MatchCollection m = properWebRequestFormat.Matches(webRequest);

            //Process Valid Requests
            if(m.Count > 0)
            {
                //Check for an IP Address to use when processing the request.
                Regex findIpAddress = new Regex(@"IPAddress=(\d+\.\d+\.\d+\.\d+)",RegexOptions.Compiled | RegexOptions.IgnoreCase);
                MatchCollection ipMatch = findIpAddress.Matches(webRequest);

                //Return Tuple with or without IPAddres where appropriate.
                if(ipMatch.Count > 0)
                    return Tuple.Create<String,String>(m[0].Groups[1].ToString(),ipMatch[0].Groups[1].ToString());
                else
                    return Tuple.Create<String,String>(m[0].Groups[1].ToString(),"");

            }
            //Request is unsupported.
            else
                return null;
        }

        //Method used for ignoring Superfulous requests.
        private static string PrepareReponse(string request)
        {
            //TODO: "request" is really spooky. Needs to be sanitizied against Cross Site Scripting, encoding crashes, etc.
            return request.IndexOf("GET") > -1 || request.IndexOf("POST") > -1 ? $"We got it: {request}" : "";

        }
    }
}
