/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace AtwaterMonitor
{
    class SocketServerTcpListener
    {
        //Code adapted from: https://msdn.microsoft.com/en-us/magazine/dn605876.aspx

        static void Main(string[] args)
        {
            try
            {
                int port = 500;
                AsyncService service = new AsyncService(port);
                service.Run(); // very specific service
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    } // Program
}*/
