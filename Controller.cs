using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using SnmpSharpNet;

namespace AtwaterMonitor
{

    class Controller
    {

       /* static void Main(string[] args)
        {
            AtwaterMonitorModel = new Model();

            foreach(string ip in UpsIpAddresses)
            {
                SnmpTest(ip);
            }

        }*/


        public bool init()
        {
            Model AtwaterMonitorModel = new Model();

            string[] UpsIpAddresses = {
                "10.10.200.110",
                "10.10.200.111",
                "10.10.200.112",
                "10.10.200.113",
                "10.10.200.114",
                "10.10.200.115",
                "10.10.180.100",
                "10.10.180.101",
                "10.10.180.102",
                "10.10.180.103",
                "10.10.180.104",
                "10.10.180.105",
                "10.10.180.106",
                "10.10.180.107",
                "10.10.180.108",
                "10.10.170.100",
                "10.10.170.101",
                "10.10.170.102",
                "10.10.170.103",
                "10.10.170.104",
                "10.10.190.100",
                "10.10.190.101",
                "10.10.190.102",
                "10.10.190.103",
                "10.10.190.104",
                "10.10.190.144",
                "10.10.190.147",
                "10.10.210.100",
                "10.10.210.101",
                "10.10.210.102",
                "10.10.210.103",
                "10.10.210.104"};

            foreach (string ip in UpsIpAddresses)
            {
                SnmpTest(ip);
            }

            //TODO Fix this.
            return true;

        }

        static void SnmpTest(string ipAddress)
        {
            Dictionary<string, string> ApcOids = new Dictionary<string, string>();

            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.1.1.1.0", "UpsType");
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.2.2.1.0", "UpsBatteryCapacity");
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.2.2.2.0", "UpsBatteryTemperature");
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.2.2.3.0", "UpsBatteryRuntimeRemaining");
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.2.2.4.0", "UpsBatteryReplace");
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.3.2.1.0", "UpsInputVoltage");
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.3.2.4.0", "UpsInputFrequency");
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.4.2.1.0", "UpsOutputVoltage");
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.4.2.2.0", "UpsOutputFrequency");
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.4.2.3.0", "UpsOutputLoad");
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.4.2.4.0", "UpsOutputCurrent");
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.7.2.4.0", "UpsDateOfLastSelfTest");

            //The following is indexed and might cause lookup problems.
            ApcOids.Add("1.3.6.1.4.1.318.1.1.25.1.2.1.5.1.1", "UpsProbeTemperatureDegF");



            /**********Credit for this code comes from the SnmpSharpNet.com webpage**********/
            /********** http://www.snmpsharpnet.com/?page_id=111 **********/

            // SNMP community name
            OctetString community = new OctetString("public");

            // Define agent parameters class
            AgentParameters param = new AgentParameters(community);
            // Set SNMP version to 1 (or 2)
            param.Version = SnmpVersion.Ver1;
            // Construct the agent address object
            // IpAddress class is easy to use here because
            //  it will try to resolve constructor parameter if it doesn't
            //  parse to an IP address
            IpAddress agent = new IpAddress(ipAddress);

            // Construct target
            UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 1);

            // Pdu class used for all requests
            Pdu pdu = new Pdu(PduType.Get);

            foreach(KeyValuePair<string,string> pair in ApcOids)
            {
                pdu.VbList.Add(pair.Key);
            }

            //pdu.VbList.Add("1.3.6.1.4.1.318.1.1.1.1.1.1.0"); //sysTemperature


            // Make SNMP request
            SnmpV1Packet result = (SnmpV1Packet)target.Request(pdu, param);

            // If result is null then agent didn't reply or we couldn't parse the reply.
            if (result != null)
            {
                // ErrorStatus other then 0 is an error returned by 
                // the Agent - see SnmpConstants for error definitions
                if (result.Pdu.ErrorStatus != 0)
                {
                    // agent reported an error with the request
                    Console.WriteLine("Error in SNMP reply. Error {0} index {1}",
                        result.Pdu.ErrorStatus,
                        result.Pdu.ErrorIndex);
                }
                else
                {
                    // Reply variables are returned in the same order as they were added
                    //  to the VbList
                    /*Console.WriteLine("sysTemperature({0}) ({1}): {2}",
                        result.Pdu.VbList[0].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[0].Value.Type),
                        result.Pdu.VbList[0].Value.ToString());*/
                    //int index = 0;
                    //Console.WriteLine(Pdu.VbList);
                    IEnumerator<Vb> test = result.Pdu.GetEnumerator();
                    do
                    {
                        if (test.Current == null)
                            continue;

                         Console.WriteLine($"{ApcOids[test.Current.Oid.ToString()],30}: {test.Current.Value.ToString(),-30}");

                        //Console.WriteLine(test.Current);

                    } while (test.MoveNext());
                    //Console.WriteLine(test.Count());
                    /*foreach(Vb i in test)
                    {

                    }
                    //foreach(KeyValuePair<string, string> pair in result.Pdu.GetEnumerator)
                    //{
                        Console.WriteLine("{0}({1}) ({2}): {3}",
                            pair.Value,
                            result.Pdu.VbList[index++].Oid.ToString(),
                            SnmpConstants.GetTypeName(result.Pdu.VbList[index++].Value.Type),
                            result.Pdu.VbList[index++].Value.ToString());*/
                    //}
                }
            }
            else
            {
                Console.WriteLine("No response received from SNMP agent.");
            }
            target.Close();

            /******** END Credited Code ********/
        }

        static void TestDriver()
        {
            Model AtwaterMonitorModel = new Model();
            float epsilon = 0.0000001f;
            UPS n = new UPS("UPS_01", @"10.10.41.41");

            n.AddTemperatureReading(89f, DateTime.Now);
            n.AddTemperatureReading(95f, DateTime.Now);
            n.AddTemperatureReading(87f, DateTime.Now);
            n.AddTemperatureReading(78f, DateTime.Now);
            AtwaterMonitorModel.AddNetworkDevice(n);

            UPS n2 = new UPS("UPS_03", @"10.10.102.41");

            n2.AddTemperatureReading(89f, DateTime.Now);
            n2.AddTemperatureReading(95f, DateTime.Now);
            n2.AddTemperatureReading(87f, DateTime.Now);
            n2.AddTemperatureReading(87.8f, DateTime.Now);

            AtwaterMonitorModel.AddNetworkDevice(n2);

            //Tests for the UPS and NetworkDevice Classes
            Debug.Assert(n.Hostname == "UPS_01", "FAILED: UPS.Constructor/Getter - Hostname");
            Debug.Assert(n.IPAddress == @"10.10.41.41", "FAILED: UPS.Constructor/Getter - IPAddress");
            Debug.Assert(n.CurrentAmbientTemperature - 78f < epsilon, "Failed: UPS.AddTemperatureReading/Current Temperature Getter");
            Debug.Assert(n.AverageAmbientTemperature - (89f + 95f + 87f + 78f) / 4f < epsilon, "FAILED: UPS.AverageTemperature/UPS.CalculateAverageTemperature/Getter");

            //Tests for the Model Class
            Debug.Assert(n2 == AtwaterMonitorModel.GetDeviceWithHostname("UPS_03"),"FAILED: Model.GetDevicesWithHostname()");
            Debug.Assert(n == AtwaterMonitorModel.GetDeviceWithIP("10.10.41.41"), "FAILED: Model.GetDeviceWithIP()");
            Debug.Assert(n2 == AtwaterMonitorModel.GetDevicesWithStatus(NetworkDevice.DeviceState.OffLine)[1]
                && n == AtwaterMonitorModel.GetDevicesWithStatus(NetworkDevice.DeviceState.OffLine)[0], "FAILED: Model.GetDevicesWithStatus()");
            Debug.Assert(n2 == AtwaterMonitorModel.GetDevicesAboveTemperature(79f)[0], "FAILED: Model.GetDevicesAboveTemperature()");

        }
    }
}
