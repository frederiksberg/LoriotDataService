using System;
using System.Collections.Generic;
using System.Text;

namespace LoriotDataConnector
{
    class CacheMessage
    {
        public string cmd { get; set; }
        public Filter filter { get; set; }
        public int page { get; set; }
        public int perPage { get; set; }
        public int total { get; set; }
        public Cache[] cache { get; set; }
    }

    public class Filter
    {
        public long from { get; set; }
        public string EUI { get; set; }
    }

    public class Cache
    {
        public string cmd { get; set; }
        public string EUI { get; set; }
        public long ts { get; set; }
        public bool ack { get; set; }
        public int fcnt { get; set; }
        public int port { get; set; }
        public string data { get; set; }
    }

}
