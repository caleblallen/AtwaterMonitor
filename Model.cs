using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtwaterMonitor
{
    class Model
    {
        private List<NetworkDevice> MonitoredDevices;
        

        public Model()
        {
            //UPS n = new UPS("UPS_01", @"10.10.41.41");
        }

        public bool AddNetworkDevice(NetworkDevice n)
        {
            MonitoredDevices.Add(n);

            //TODO: Return false if something fails.
            return true;
        }

        public bool AddNetworkDevice()
        {

            return true;
        }

        
    }



}
