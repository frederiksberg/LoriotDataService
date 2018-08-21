using System;
using System.Collections.Generic;
using System.Text;

namespace LoriotDataConnector
{
    class GatewayInformationFrame
    {
        public string cmd { get; set; }
        public string EUI { get; set; }
        public long ts { get; set; }
        public bool ack { get; set; }
        public int fcnt { get; set; }
        public int port { get; set; }
        public string data { get; set; }
        public int freq { get; set; }
        public string dr { get; set; }
        public Gateway[] gws { get; set; }
    }

    public class Gateway
    {
        public int rssi { get; set; }
        public float snr { get; set; }
        public long ts { get; set; }
        public string gweui { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
    }

}

