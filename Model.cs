using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtwaterMonitor
{
    class Model
    {
        /* This class acts as the Model for our application. It stores and updates all information relevant
         * to our program. 
         */

        private List<NetworkDevice> MonitoredDevices;
        
        //Constructor: Initialize the list of monitored devices.
        public Model()
        {
            MonitoredDevices = new List<NetworkDevice>();
        }

        //Add a device to the list of Monitored Devices. Returns index of added device.
        public int AddNetworkDevice(NetworkDevice n)
        {
            MonitoredDevices.Add(n);
            return MonitoredDevices.Count() - 1;
        }

        public IEnumerator<NetworkDevice> getDeviceEnumerator()
        {
            return MonitoredDevices.GetEnumerator();
        }

        //Return the device at a specific index. Will allow the view to grab context specific devices.
        public NetworkDevice GetDeviceAtIndex(int index)
        {
            return MonitoredDevices[index];
        }

        //Return the first device with specific Hostname. Unnecissary?
        public NetworkDevice GetDeviceWithHostname(string hostname)
        {
            return MonitoredDevices.Find(x => x.Hostname.Contains(hostname));
        }

        //Return the first device with a specific IP Address. Unnecisssary?
        public NetworkDevice GetDeviceWithIP(string ipAddress)
        {
            return MonitoredDevices.Find(x => x.IPAddress.Contains(ipAddress));
        }

        //Return a List of NetworkDevices with the specified DeviceState.
        public List<NetworkDevice> GetDevicesWithStatus(NetworkDevice.DeviceState state)
        {
            return MonitoredDevices.FindAll(x => x.State == state);
        }

        //Extract and return just UPS devices. Better or worse than separate list???
        public List<UPS> GetUPSDeviceEnumerator()
        {
            return MonitoredDevices.Where(x => x is UPS).Select(x => (UPS)x).ToList();
        }

        //Extract and return list of UPS devices with CurrentTemperuature above temp. 
        public List<UPS> GetDevicesAboveTemperature(float temp)
        {
            List<UPS> upsDevices = this.GetUPSDeviceEnumerator();

            return upsDevices.FindAll(x => x.CurrentAmbientTemperature > temp);
        }


        //Extract and return list of UPS devices with CurrentTemperature below temp
        public List<UPS> GetDevicesBelowTemperature(float temp)
        {
            List<UPS> upsDevices = this.GetUPSDeviceEnumerator();

            return upsDevices.FindAll(x => x.CurrentAmbientTemperature < temp);
        }
    }
}
