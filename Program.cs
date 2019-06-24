using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sandbox2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World");

            UPS n = new UPS("UPS_01", @"10.10.41.41");

            n.AddTemperatureReading(78f, DateTime.Now);
            n.AddTemperatureReading(89f, DateTime.Now);
            n.AddTemperatureReading(95f, DateTime.Now);
            n.AddTemperatureReading(87f, DateTime.Now);
            n.AddTemperatureReading(78f, DateTime.Now);

            Console.WriteLine(n);
        }
    }

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
        private DeviceState State { get; set; }

        //Network Location Info
        public string Hostname { get; set; }
        private string IPAddress { get; set; }

        //Constructor
        public NetworkDevice (string hostname, string ip, DeviceState state = DeviceState.OffLine)
        {
            this.Hostname = hostname;
            this.IPAddress = ip;
            this.State = state;
        }

        public NetworkDevice() : this(hostname:"UNKNOWN", ip:"UNKNOWN",state:DeviceState.OffLine)
        {
        }
        
        //ToString dumps everything we know to the console.
        public override string ToString()
        {
            return $"Device Details:\n" + 
                    $"\tHostname:\t{this.Hostname}\n" + 
                    $"\tIP Address:\t{this.IPAddress}\n" + 
                    $"\tStatus:\t\t{this.State}\n";
        }


        /*
         * What do I want here?
         *  1) Network access for this device (done)
         *  2) Needs to know its current temperature
         *  3) Needs to know its highest temperature within the last 24 hours???
         * 
         */
    }

    class UPS : NetworkDevice
    {
        //Track the max temperature readings we want to keep per device.
        static int MaxHistoryLength = 48;

        //Temperature readings are value pair. Temperature and Time of Reading
        internal struct TemperatureReading
        {
            internal float temp;
            internal DateTime timeStamp;

            public TemperatureReading(float temperature, DateTime time)
            {
                this.temp = temperature;
                this.timeStamp = time;
            }
        }

        public float AverageTemperature { get; private set; }
        public float CurrentTemperature { get; private set; }

        //Temperature logs. 
        private Queue<TemperatureReading> TemperatureHistory = new Queue<TemperatureReading>();

        public UPS (
            string hostname, 
            string ip, 
            DeviceState state = DeviceState.OffLine, 
            float temperature = 0.0f ) : base (hostname, ip, state)
        {
            //Add the passed Current Temperature
            this.CurrentTemperature = temperature;

            //There is only one temperature reading, so we pass it to the average as well.
            this.AverageTemperature = temperature;

            //Log the passed current temperature in the History Queue.
            TemperatureHistory.Enqueue(new TemperatureReading(temperature: temperature, time: DateTime.Now));

        }

        public UPS() : this(hostname: "UNKNOWN", ip: "UNKNOWN") { }

        private void CalculateAverageTemperature()
        {
            AverageTemperature = TemperatureHistory.Average(t => t.temp);
        }

        public bool AddTemperatureReading(float temp, DateTime timeStamp)
        {
            
            //Dequeue to make room if we reach the maximum number of entries
            while (TemperatureHistory.Count() > MaxHistoryLength)
                TemperatureHistory.Dequeue();

            //Add our new temperature reading to the queue
            TemperatureHistory.Enqueue(new TemperatureReading(temperature: temp, time: timeStamp));

            //Track the Current Temperature
            CurrentTemperature = temp;

            //Recalculate the average temperature since we've altered the log.
            CalculateAverageTemperature();

            //TODO Return success or failure
            return true;
        }

        public override string ToString()
        {

            return base.ToString() + 
                $"\tAverage Temp:\t{this.AverageTemperature}°F\n" +
                $"\tCurrent Temp:\t{this.CurrentTemperature}°F\n";
        }
    }


}
