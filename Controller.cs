using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtwaterMonitor
{

    class Controller
    {
        static void Main(string[] args)
        {
            UPS n = new UPS("UPS_01", @"10.10.41.41");

            n.AddTemperatureReading(78f, DateTime.Now);
            n.AddTemperatureReading(89f, DateTime.Now);
            n.AddTemperatureReading(95f, DateTime.Now);
            n.AddTemperatureReading(87f, DateTime.Now);
            n.AddTemperatureReading(78f, DateTime.Now);

            Console.WriteLine(n);
        }
    }
}
