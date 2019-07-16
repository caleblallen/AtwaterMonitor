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

        //public Func<AMProgram.WebRequestType,string> SignalCallback;

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

        public async void Run(ServiceCallback callback)
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
                    Task t = Process(tcpClient,callback);
                    await t; // could combine with above
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        } // Start

        private async Task Process(TcpClient tcpClient, ServiceCallback callback)
        {
            string clientEndPoint = tcpClient.Client.RemoteEndPoint.ToString();
            Console.WriteLine("Received connection request from " + clientEndPoint);


            bool done = false;
            try
            {
                NetworkStream networkStream = tcpClient.GetStream();
                StreamReader reader = new StreamReader(networkStream);
                StreamWriter writer = new StreamWriter(networkStream);
                writer.AutoFlush = true;
                while (!done)
                {
                    string request = await reader.ReadLineAsync();
                    if (request != null)
                    {
                        Console.WriteLine("Received service request: " + request);
                        string response = PrepareReponse(request);
                        Console.WriteLine("Computed response is: " + response + "\n");

                        if (!string.IsNullOrEmpty(response))
                        {

                            string r = callback.Invoke(0, "10.10.10.10");
                            StringBuilder res = new StringBuilder("HTTP/1.1 200 OK\r\n");
                            res.Append("Content-Type: text/*\r\n");
                            res.Append("\r\n");
                            res.AppendFormat(r);
                            await writer.WriteLineAsync(res.ToString());
                            tcpClient.Dispose();
                            done = true;
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
            return request.IndexOf("GET") > -1 || request.IndexOf("POST") > -1 ? $"We got it: {request}" : "";

/*
            if (request.IndexOf("GET") > -1 || request.IndexOf("POST") > -1)
                return $"We got it: {request}";
            else
                return "";*/
        }
    }
}
