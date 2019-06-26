using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtwaterMonitor
{
    class AMProgram
    {
        public static void Main(string[] args)
        {
            Controller AtwaterMonitorController = new Controller();

            AtwaterMonitorController.init();
        }
    }
}
