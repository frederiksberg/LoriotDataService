using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoriotDataConnector
{
    class DataContext : DbContext
    {
        public DbSet<DecentSensorFrame> DecentFrames { get; set; }
        public DbSet<GatewayInformation> GatewayInformation { get; set; }

        public DataContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("postgis");
            base.OnModelCreating(modelBuilder);
        }
    }
}
