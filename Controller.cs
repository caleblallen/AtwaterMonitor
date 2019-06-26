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
    public class ImproperOidsException : Exception
    {
        public ImproperOidsException()
        {
        }
        public ImproperOidsException(string message) : base(message)
        {
        }

        public ImproperOidsException(string message, Exception inner) : base(message, inner)
        {
        }
    }
    class Controller
    { //SnmpSharpNet.SnmpException
        private Dictionary<string, string> ApcOids;
        private string[] UpsIpAddresses;

        public bool init()
        {
            //Can't forget about our unit tests
            TestDriver();

            //Create the master Model
            Model AtwaterMonitorModel = new Model();

            //Initialize and Fill the ApcOids Dictionary
            //TODO: Move these OIDs to a config file of some sort.
            ApcOids = new Dictionary<string, string>();
            //ApcOids.Add("1.3.6.1.4.1.318.1.1.1.1.1.1", "UpsType"); 
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.1.1.1.0", "UpsType"); 
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.1.1.2.0", "UpsName");
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.1.2.3.0", "UpsSerialNumber");
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.2.2.2.0", "UpsBatteryTemperature");
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.3.2.1.0", "UpsInputVoltage");
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.3.2.4.0", "UpsInputFrequency");
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.4.2.1.0", "UpsOutputVoltage");
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.4.2.2.0", "UpsOutputFrequency");
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.7.2.4.0", "UpsDateOfLastSelfTest");
            ApcOids.Add("1.3.6.1.4.1.318.1.1.1.7.2.3.0", "UpsResultsOfLastSelfTest"); //1: ok, 2: failed, 3: invalid, 4: in progress
            
            //The following is indexed and might cause lookup problems.
            //ApcOids.Add("1.3.6.1.4.1.318.1.1.25.1.2.1.5.1.1", "UpsProbeTemperatureDegF");

            //Initialize our UPS IPAddresses
            UpsIpAddresses = new string[] {
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
                Console.WriteLine(CreateApcUpsAtIp(ip));
                //SnmpTest(ip);
            }

            //TODO Fix this.
            return true;

        }

        private UPS CreateApcUpsAtIp(string ipAddress)
        {
            

            //The last two digits of the Temp Probe OID are an index. Port # then Sensor #. When in doubt, force it.
            string[] ApcUpsProbTemperatureDegFOids = { "1.3.6.1.4.1.318.1.1.25.1.2.1.5.1.1", "1.3.6.1.4.1.318.1.1.25.1.2.1.5.2.1",
                                                    "1.3.6.1.4.1.318.1.1.25.1.2.1.5.1.2", "1.3.6.1.4.1.318.1.1.25.1.2.1.5.2.2"};
            //No choice but to check and see if the device is there. Two possible exception paths that must be handled.
            try
            {
                GetSnmpInfo(ipAddress, ApcOids.Keys);
            }
            //An SnmpException means we either have no device to reach, or we're hitting a device without SNMP v1 running and responding.
            catch (SnmpException e)
            {
                Console.WriteLine($"Device at {ipAddress} is not responding to SNMP Calls:\n{e.Message}");
                return null;
            }
            //An ImproperOidsException means we are asking for Object IDentifiers the target is unaware of.
            catch (ImproperOidsException e)
            {
                Console.WriteLine($"Device at {ipAddress} is not responding to the configured OIDs:\n{e.Message}");
                return null;
            }

            ApcUpsProbTemperatureDegFOids.GetEnumerator();
            return null;
        }

        private void GetSnmpInfo(string ipAddress, IEnumerable<string> oids)
        {

            /********** Heavily Modified code from the SnmpSharpNet webpage **********/
            /**********      http://www.snmpsharpnet.com/?page_id=111       **********/

            // SNMP community name
            OctetString community = new OctetString("public");

            // Define agent parameters class
            AgentParameters param = new AgentParameters(community);

            // Set SNMP version to 1
            param.Version = SnmpVersion.Ver1;

            // Construct the agent address object
            IpAddress agent = new IpAddress(ipAddress);

            // Construct target
            UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 1);

            // Pdu class used for all requests
            Pdu pdu = new Pdu(PduType.Get);

            //Add list of all relevant OIDs
            foreach (string key in oids)
            {
                pdu.VbList.Add(key);
            }

            // Make SNMP request
            SnmpV1Packet result = (SnmpV1Packet)target.Request(pdu, param);

            // If result is null then agent didn't reply or we couldn't parse the reply.
            if (result != null)
            {
                // ErrorStatus other then 0 is an error returned by 
                // the Agent - see SnmpConstants for error definitions
                if (result.Pdu.ErrorStatus != 0)
                {
                    //Close the connection since we're going to throw an exception.
                    target.Close();

                    // agent reported an error with the request
                    string msg = $"OID {oids.ElementAt(result.Pdu.ErrorIndex)} "
                                 + "has exited with error status "
                                 + $"{ result.Pdu.ErrorStatus}: {Enum.GetName(typeof(PduErrorStatus), result.Pdu.ErrorStatus)}";
                    throw new ImproperOidsException(msg);
                }
                else
                {
                    //Print all information we gleaned from inspecting the UPS
                    IEnumerator<Vb> EnumeratedResults = result.Pdu.GetEnumerator();
                    do
                    {
                        if (EnumeratedResults.Current == null)
                            continue;

                        Console.WriteLine($"{ApcOids[EnumeratedResults.Current.Oid.ToString()],30}: {EnumeratedResults.Current.Value.ToString(),-30}");

                    } while (EnumeratedResults.MoveNext());

                }
            }
            else
            {
                Console.WriteLine("No response received from SNMP agent.");
            }
            target.Close();

            /******** END Credited Code ********/
        }


        private void SnmpTest(string ipAddress)
        {

            /********** Heavily Modified code from the SnmpSharpNet webpage **********/
            /**********      http://www.snmpsharpnet.com/?page_id=111       **********/

            // SNMP community name
            OctetString community = new OctetString("public");

            // Define agent parameters class
            AgentParameters param = new AgentParameters(community);
            
            // Set SNMP version to 1
            param.Version = SnmpVersion.Ver1;
           
            // Construct the agent address object
            IpAddress agent = new IpAddress(ipAddress);

            // Construct target
            UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 1);

            // Pdu class used for all requests
            Pdu pdu = new Pdu(PduType.Get);

            //Add list of all relevant OIDs
            foreach(KeyValuePair<string,string> pair in ApcOids)
            {
                pdu.VbList.Add(pair.Key);
            }

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
                    //Print all information we gleaned from inspecting the UPS
                    IEnumerator<Vb> EnumeratedResults = result.Pdu.GetEnumerator();
                    do
                    {
                        if (EnumeratedResults.Current == null)
                            continue;

                         Console.WriteLine($"{ApcOids[EnumeratedResults.Current.Oid.ToString()],30}: {EnumeratedResults.Current.Value.ToString(),-30}");

                    } while (EnumeratedResults.MoveNext());

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
