using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtwaterMonitor
{
    public enum WebRequestType 
    {
        GetTemperature
    };

    class AMProgram
    {
        private const int ListenPort = 3000;


        public static void Main(string[] args)
        {
            Controller AtwaterMonitorController = new Controller();

            
            try
            {
                AsyncService service = new AsyncService(ListenPort);
                service.Run(AtwaterMonitorController.GetHttpRequestHandler()); 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            AtwaterMonitorController.init();

            Console.ReadLine();
        }


    }
}
