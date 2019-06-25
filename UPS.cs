﻿using System;
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

        public UPS(
            string hostname,
            string ip,
            DeviceState state = DeviceState.OffLine,
            float temperature = 0.0f) : base(hostname, ip, state)
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

            //TODO: Return success or failure. (can this fail?)
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