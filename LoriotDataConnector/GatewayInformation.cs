using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoriotWebsocketClient
{
    class GatewayInformation
    {
        public long Id { get; set; }

        public string GatewayId { get; set; }

        public int Rssi { get; set; }

        public float Snr { get; set; }

        public Point Location { get; set; }
    }
}
