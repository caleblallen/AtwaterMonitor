using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtwaterMonitor
{
    //Requests we weill actually respond to.
    public enum WebRequestType 
    {
        GetTemperature,
        GetAllTemperatures,

    };

    //Program Entry Point
    class AMProgram
    {
        private const int ListenPort = 3000;

        public static void Main(string[] args)
        {
            //TODO: Implement Config file for future customization.

            //Create the MVC controller.
            Controller AtwaterMonitorController = new Controller();

            
            try
            {
                //Create our TCP web service.
                AsyncService service = new AsyncService(ListenPort);
                
                //Run the server and send in a delegate from the Controller for handling requests.
                service.Run(AtwaterMonitorController.GetHttpRequestHandler()); 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            AtwaterMonitorController.run();

            //Keep the program running.
            Console.ReadLine();
        }


    }
}
