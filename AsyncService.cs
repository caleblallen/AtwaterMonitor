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


            this.ipAddress = IPAddress.Parse("10.10.42.80");

            if (this.ipAddress == null)
                throw new Exception("No IPv4 address for server");
            Console.WriteLine("IP Address is {0}", this.ipAddress.ToString());
        }

        public async void Run()
        {
            TcpListener listener = new TcpListener(this.ipAddress, this.port);
            listener.Start();
            Console.WriteLine("\nArray Min and Avg service is now running on port " + this.port);
            Console.WriteLine("Hit <enter> to stop service\n");

            while (true)
            {
                try
                {
                    //Console.WriteLine("Waiting for a request ..."); 
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                    Task t = Process(tcpClient);
                    await t; // could combine with above
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        } // Start

        private async Task Process(TcpClient tcpClient)
        {
            string clientEndPoint = tcpClient.Client.RemoteEndPoint.ToString();
            Console.WriteLine("Received connection request from " + clientEndPoint);

            try
            {
                NetworkStream networkStream = tcpClient.GetStream();
                StreamReader reader = new StreamReader(networkStream);
                StreamWriter writer = new StreamWriter(networkStream);
                writer.AutoFlush = true;
                while (true)
                {
                    string request = await reader.ReadLineAsync();
                    if (request != null)
                    {
                        Console.WriteLine("Received service request: " + request);
                        string response = PrepareReponse(request);
                        Console.WriteLine("Computed response is: " + response + "\n");

                        if (!string.IsNullOrEmpty(response))
                        {
                            StringBuilder res = new StringBuilder("HTTP/1.1 200 OK\r\n");
                            res.Append("Content-Type: text/*\r\n");
                            res.Append("\r\n");
                            res.AppendFormat("Your Computed Response is: {0}", response);
                            await writer.WriteLineAsync(res.ToString());
                        }

                    }
                    else
                        break; // client closede connection
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

        private static string PrepareReponse(string request)
        {
            //TODO: "request" is really spooky. Needs to be sanitizied against Cross Site Scripting, encoding crashes, etc.
            Regex rx = new Regex(@"GET\s/(.*?)/(.*?)\s.*");
            MatchCollection requestMatches = rx.Matches(request);

            if (requestMatches.Count > 0)
            {
                foreach (Match m in rx.Matches(request))
                {
                    return $"{m.Groups[1]} --> {m.Groups[2]}";
                }
            }

            return "";
        }
    }
}
