using System;
using Crestron.SimplSharp;
using System.Linq;




namespace FourSeriesConsoleOperations
{
    public class ConsoleInterface
    {
        public string ConsoleRsp;
        /// <summary>
        /// Performs an Ethernet Autodiscovery on the defined adapter
        /// </summary>
        /// <param name="adapter"></param>
        public void PerformAutoDiscoveryQuery(string adapter)
        {
            //set adapter variable to LAN or CS according to argument of method
            EthernetAdapterType _adapter = new EthernetAdapterType();

            if (adapter == "CS")
            {
                _adapter = EthernetAdapterType.EthernetCSAdapter;
            }
            else
            {
                _adapter = EthernetAdapterType.EthernetLANAdapter;
            }

            //index for simpl+ to reference for array building...Simpl+ arrays are 1 based
            int i = 0;

            try
            {
                //run autodiscovery query and if successful loop thru items
                if (EthernetAutodiscovery.Query(_adapter) == EthernetAutodiscovery.eAutoDiscoveryErrors.AutoDiscoveryOperationSuccess)
                {

                    //loop thru all items in sorted DiscoveredElementsList. sorting ensures that the list is always ordered the same
                    foreach (EthernetAutodiscovery.AutoDiscoveredDeviceElement device in EthernetAutodiscovery.DiscoveredElementsList.OrderBy(d => d.HostName))
                    {
                        //only do this for devices that report their mac in the deviceID field. This omits DM endpoints and flex computes
                        if (device.DeviceIdString.Contains("@E-"))
                        {
                            //Create an event arg from the device details
                            DiscoveryElementArgs args = new DiscoveryElementArgs
                            {
                                Index = (ushort)(i + 1),
                                Hostname = device.HostName,
                                IPAddress = device.IPAddress,
                                DeviceID = device.DeviceIdString.Substring(device.DeviceIdString.Length - 12)
                            };

                            //check to see if the event is not null (it exists) and then fire the event off with the event args we created above
                            if (!DeviceAddedEventCall.Equals(null))
                            {
                                DeviceAddedEventCall(this, args);
                            }

                            //increment the index
                            i++;
                        }
                    }
                }
            }
            catch (NotSupportedException ex)
            {
                CrestronConsole.PrintLine("Ethernet Autodiscovery is not supported on this system.\n{0}", ex.Message);
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Error in PerformAutoDiscoveryQuery: {0}", e.Message);
            }
        }

        /// <summary>
        /// Method to send any cmd to console and receive response.
        /// </summary>
        /// <param name="cmd"></param>
        public void SendCustomConsoleCmd(string cmd)
        {
            string[] rspLines;

            try
            {

                if (CrestronConsole.SendControlSystemCommand(cmd, ref ConsoleRsp))
                {

                    ConsoleRspEventArgs args = new ConsoleRspEventArgs();
                    args.Response = ConsoleRsp;
                    
                    //a way to see the response line by line.
                    rspLines = ConsoleRsp.Split('\n');

                    //initialize array
                    args.ResponseList = new string[rspLines.Length];

                    //args.ResponseList.Initialize();
                    if (args.ResponseList.Length > 0)
                    {
                        int i = 0;
                        foreach (var line in rspLines)
                        {
                            args.ResponseList[i] = line;
                            CrestronConsole.PrintLine(line);
                            i++;
                        }

                        args.ResponseListCount = (ushort)args.ResponseList.Length;
                    }
                    
                    if (!ConsoleRspEventCall.Equals(null))
                    {
                        ConsoleRspEventCall(this, args);
                    }

                }
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Error in SendCustomConsoleCmd: {0}",e.Message);
            }

        }

        /// <summary>
        /// eventhandler for when a new Ethernet Autodiscovery Query is processed
        /// </summary>
        public event EventHandler<DiscoveryElementArgs> DeviceAddedEventCall;

        /// <summary>
        /// eventhandler for when a new Ethernet Autodiscovery Query is processed
        /// </summary>
        public event EventHandler<ConsoleRspEventArgs> ConsoleRspEventCall;
    }
}


