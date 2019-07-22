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
                if (ipHostInfo.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
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

                //Flag to finish operations
                bool done = false; 

                while (!done)
                {
                    //Read a line of incoming data
                    string request = await reader.ReadLineAsync();

                    //Process valid requests.
                    if (request != null)
                    {
                        //Notify the console the request was received.
                        //TODO: Remove in Version 1.
                        Console.WriteLine("Received service request: " + request);

                        //Prepare resonses and ignore superfluous ones.
                        string response = PrepareReponse(request);

                        if (!string.IsNullOrEmpty(response))
                        {
                            //Invoke the callback that corresponds to Controller.HttpRequestHandler();
                            string dataFromController = callback.Invoke(WebRequestType.GetAllTemperatures,"");

                            //Builder to prepare our response.
                            StringBuilder res = new StringBuilder("HTTP/1.1 200 OK\r\n");

                            //TODO: restrict cross origin to webserver only.
                            res.Append("Access-Control-Allow-Origin: *\r\n"); 
                            res.Append("Content-Type: text/*\r\n");
                            res.Append("\r\n");
                            res.Append(dataFromController);
                            await writer.WriteLineAsync(res.ToString());

                            //Currently, we end operation after sending data. Client may make a new request if needed.
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

        //Method used for ignoring Superfulous requests.
        private static string PrepareReponse(string request)
        {
            //TODO: "request" is really spooky. Needs to be sanitizied against Cross Site Scripting, encoding crashes, etc.
            return request.IndexOf("GET") > -1 || request.IndexOf("POST") > -1 ? $"We got it: {request}" : "";

        }
    }
}
