using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtwaterMonitor
{

    class NetworkDevice
    {
        //Device State tracking for the Model.
        public enum DeviceState
        {
            OnLine,
            OffLine,
            Warning,
            Alarm
        }
        public DeviceState State { get; set; }

        //Network Location Info
        public string Hostname { get; private set; }
        public string IPAddress { get; private set; }

        //Constructor
        public NetworkDevice(string hostname, string ip, DeviceState state = DeviceState.OffLine)
        {
            this.Hostname = hostname;
            this.IPAddress = ip;
            this.State = state;
        }

        public NetworkDevice() : this(hostname: "UNKNOWN", ip: "UNKNOWN", state: DeviceState.OffLine) { }

        //ToString dumps everything we know to the console.
        public override string ToString()
        {
            return $"Device Details:\n" +
                    $"\tHostname:\t{this.Hostname}\n" +
                    $"\tIP Address:\t{this.IPAddress}\n" +
                    $"\tStatus:\t\t{this.State}\n";
        }
    }
}
