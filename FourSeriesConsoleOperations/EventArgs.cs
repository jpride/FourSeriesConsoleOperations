using System;

namespace FourSeriesConsoleOperations
{
    public class DiscoveryElementArgs : EventArgs
    {
        public ushort Index { get; set; }
        public string Hostname { get; set; }
        public string IPAddress { get; set; }
        public string DeviceID { get; set; }
    }

    public class ConsoleRspEventArgs : EventArgs
    { 
        public string Response { get; set; }
    }
}
