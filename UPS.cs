using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtwaterMonitor
{
    class UPS : NetworkDevice
    {
        //Track the max temperature readings we want to keep per device.
        static int MaxHistoryLength = 48;

        //Object Identifiers for APC UPS Units
        public static string[] Oids =
        {
            "1.3.6.1.4.1.318.1.1.1.1.1.1.0", //APC Model Name/Number
            "1.3.6.1.4.1.318.1.1.1.1.1.2.0", //APC Hostname
            "1.3.6.1.4.1.318.1.1.1.1.2.3.0", //APC Serial Number
            "1.3.6.1.4.1.318.1.1.1.2.2.2.0", //APC Battery Temperature
            "1.3.6.1.4.1.318.1.1.1.3.2.1.0", //APC Input Voltage
            "1.3.6.1.4.1.318.1.1.1.3.2.4.0", //APC Input Frequency
            "1.3.6.1.4.1.318.1.1.1.4.2.1.0", //APC Output Voltage
            "1.3.6.1.4.1.318.1.1.1.4.2.2.0", //APC Output Frequency
            "1.3.6.1.4.1.318.1.1.1.7.2.4.0", //APC Date of Last Self Test
            "1.3.6.1.4.1.318.1.1.1.7.2.3.0", //APC Results of Last Self Test
        };

        //Potential Object Identifiers for the this UPS's Temperature Probe
        public static string[] TemperatureProbeOids =
        {
            "1.3.6.1.4.1.318.1.1.25.1.2.1.5.1.1", //APC Universal IO Port 1, Sensor 1
            "1.3.6.1.4.1.318.1.1.25.1.2.1.5.2.1", //APC Universal IO Port 2, Sensor 1
            "1.3.6.1.4.1.318.1.1.25.1.2.1.5.1.2", //APC Universal IO Port 1, Sensor 2
            "1.3.6.1.4.1.318.1.1.25.1.2.1.5.2.2"  //APC Universal IO Port 2, Sensor 2
        };

        //Model Number of the UPS
        public string Model { get; set; }

        //Serial Number of the UPS
        public string SerialNumber { get; }

        //Store the Temperature Probe's Oid. It changes depending on which port it is plugged in to.
        public string TemperatureSensorOid { get; private set; }

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

        public float AverageAmbientTemperature { get; private set; }
        public float CurrentAmbientTemperature { get; private set; }

        //Temperature logs. 
        private Queue<TemperatureReading> AmbientTemperatureHistory = new Queue<TemperatureReading>();

        public UPS(
            string hostname,
            string ip,
            DeviceState state = DeviceState.OffLine,
            float temperature = 0.0f) : base(hostname, ip, state)
        {
            //Add the passed Current Temperature
            this.CurrentAmbientTemperature = temperature;

            //There is only one temperature reading, so we pass it to the average as well.
            this.AverageAmbientTemperature = temperature;

            //Log the passed current temperature in the History Queue.
            AmbientTemperatureHistory.Enqueue(new TemperatureReading(temperature: temperature, time: DateTime.Now));

        }

        public UPS(
            string hostname,
            string ip,
            string model,
            string serialNumber,
            string tempSensorOid = "",
            DeviceState state = DeviceState.OffLine,
            float temperature = 0.0f) : base(hostname, ip, state)
        {
            //Add the passed Current Temperature
            this.CurrentAmbientTemperature = temperature;

            //There is only one temperature reading, so we pass it to the average as well.
            this.AverageAmbientTemperature = temperature;

            this.SerialNumber = serialNumber;

            this.Model = model;

            this.TemperatureSensorOid = tempSensorOid;

            //Log the passed current temperature in the History Queue.
            AmbientTemperatureHistory.Enqueue(new TemperatureReading(temperature: temperature, time: DateTime.Now));

        }

        public UPS() : this(hostname: "UNKNOWN", ip: "UNKNOWN") { }

        private void CalculateAverageTemperature()
        {
            AverageAmbientTemperature = AmbientTemperatureHistory.Average(t => t.temp);
        }


        public bool AddAmbientTemperatureReading(float temp, DateTime timeStamp)
        {

            //Dequeue to make room if we reach the maximum number of entries
            while (AmbientTemperatureHistory.Count() > MaxHistoryLength)
                AmbientTemperatureHistory.Dequeue();

            //Add our new temperature reading to the queue
            AmbientTemperatureHistory.Enqueue(new TemperatureReading(temperature: temp, time: timeStamp));

            //Track the Current Temperature
            CurrentAmbientTemperature = temp;

            //Recalculate the average temperature since we've altered the log.
            CalculateAverageTemperature();

            //TODO: Return success or failure. (can this fail?)
            return true;
        }

        public override string ToString()
        {

            return base.ToString() +
                $"\tModel:\t{this.Model}\n" +
                $"\tSerial Number:\t{this.SerialNumber}\n" +
                $"\tTemperature Sensor Oid:\t{this.TemperatureSensorOid}\n" +
                $"\tAverage Temp:\t{this.AverageAmbientTemperature}°F\n" +
                $"\tCurrent Temp:\t{this.CurrentAmbientTemperature}°F\n";
        }
    }

}
