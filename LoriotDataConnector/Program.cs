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
using Microsoft.Extensions.Logging;

namespace LoriotDataConnector
{
    class Program
    {
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

                var token = hostContext.Configuration.GetSection("Settings").GetValue<string>("LoriotToken");
                services.AddSingleton(x => new LoriotWebsocketHandler(token));

                var conString = hostContext.Configuration.GetConnectionString("DatebaseDefault");              
                services.AddDbContext<DataContext>(options => options.UseNpgsql(conString, o => o.UseNetTopologySuite()));
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


            var logger = host.Services.GetService<ILogger<Program>>();

            AppDomain.CurrentDomain.UnhandledException += (sender, ex) => logger.LogError(ex.ExceptionObject as Exception, "An exception occured");

            await host.RunAsync();
        }
    }
}
