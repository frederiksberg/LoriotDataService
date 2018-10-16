using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LoriotWebsocketClient
{
    [Table("water_levels")]
    class WaterLevel
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("device_id")]
        public int DeviceId { get; set; }

        [Column("pressure")]
        public float Pressure { get; set; }

        [Column("temperature")]
        public float Temperature { get; set; }

        [Column("battery")]
        public float Battery { get; set; }

        [Column("timestamp")]
        public DateTimeOffset TimeStamp { get; set; }
    }
}
