using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebSocketSharp;
using NLog.Extensions.Logging;

namespace LoriotDataConnector
{
    class Program
    {
        private static string _conString = "Server=srwebgisadm01;Port=5432;Database=loriot;User Id = gc2; Password=Ta2ezatr;";

        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
            .ConfigureHostConfiguration(configHost =>
            {
                configHost.SetBasePath(Directory.GetCurrentDirectory());
                configHost.AddJsonFile("hostsettings.json");
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<LoriotDataService>();
                services.AddDbContext<DataContext>(options => options.UseNpgsql(_conString, o => o.UseNetTopologySuite()));
            })
            .ConfigureLogging((hostContext, configLogging) =>
             {
                 configLogging.AddNLog();
             })
             .UseConsoleLifetime()
            .Build();

            var db = host.Services.GetService<DataContext>();

            db.Database.EnsureDeleted();

            db.Database.EnsureCreated();

            await host.RunAsync();
        }

    }
}
