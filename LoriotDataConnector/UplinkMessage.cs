using System;
using System.Collections.Generic;
using System.Text;

namespace LoriotDataConnector
{

    public class UplinkMessage
    {
        public long id { get; set; }

        public string cmd { get; set; }
        public int seqno { get; set; }
        public string EUI { get; set; }
        public long ts { get; set; }
        public int fcnt { get; set; }
        public int port { get; set; }
        public int freq { get; set; }
        public int rssi { get; set; }
        public float snr { get; set; }
        public int toa { get; set; }
        public string dr { get; set; }
        public bool ack { get; set; }
        public int bat { get; set; }
        public string data { get; set; }
    }

}
