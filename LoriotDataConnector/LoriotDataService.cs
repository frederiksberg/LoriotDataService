using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace LoriotDataConnector
{
    class LoriotDataService : IHostedService
    {
        private Microsoft.Extensions.Logging.ILogger _logger;
        private DataContext _db;
        private WebSocket _webSocket;

        public LoriotDataService(ILogger<LoriotDataService> logger, DataContext context)
        {
            _logger = logger;
            _db = context;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Service...");

            _webSocket = new WebSocket("wss://iotnet.teracom.dk/app?token=vnoQXQAAABFpb3RuZXQudGVyYWNvbS5ka1r5y5z9-lZZzyA8ZRVR4jI=");
            _webSocket.OnMessage += OnMessageReceived;

            _webSocket.Connect();

            _logger.LogInformation("Service connected to websocket");

            _webSocket.Send("{\"cmd\":\"cq\"}");



            return Task.CompletedTask;
        }

        void OnMessageReceived(object sender, MessageEventArgs e)
        {
            var command = JsonConvert.DeserializeObject<Command>(e.Data);


            _logger.LogInformation($"Received Message: {command.cmd}");

            switch (command.cmd)
            {
                case "rx":
                    HandleUplinkMessage(e);
                    break;
                case "gw":
                    HandleGatewayInformation(e);
                    break;
                default:
                    break;
            }


        }

        private void HandleGatewayInformation(MessageEventArgs e)
        {
            var message = JsonConvert.DeserializeObject<GatewayInformationFrame>(e.Data);

            var gatewayInfo = message.gws.Select(x => new GatewayInformation
            {
                GatewayId = x.gweui,
                Rssi = x.rssi,
                Snr = x.rssi,
                Location = new NetTopologySuite.Geometries.Point(x.lat, x.lon)
            });

            _db.GatewayInformation.AddRange(gatewayInfo);
            _db.SaveChanges();
        }

        private void HandleUplinkMessage(MessageEventArgs e)
        {
            var uplinkMessage = JsonConvert.DeserializeObject<UplinkMessage>(e.Data);
            var mesg = DecodeFrame(uplinkMessage);
            _db.DecentFrames.Add(mesg);
            _db.SaveChanges();
        }

        DecentSensorFrame DecodeFrame(UplinkMessage message)
        {
            string[] hexValuesSplit = Split(message.data, 2).ToArray();

            var version = Convert.ToInt32(hexValuesSplit[0], 16);
            var deviceId = Convert.ToInt32(hexValuesSplit[1] + hexValuesSplit[2], 16);
            var flags = Convert.ToInt32(hexValuesSplit[3] + hexValuesSplit[4], 16);
            var pressure = Convert.ToInt32(hexValuesSplit[5] + hexValuesSplit[6], 16);
            var temperature = Convert.ToInt32(hexValuesSplit[7] + hexValuesSplit[8], 16);
            var battery = Convert.ToInt32(hexValuesSplit[9] + hexValuesSplit[10], 16);

            var realPressure = ((float)pressure - 16384) / 32768 * (1 - 0) + 0;
            var realTemperature = (((float)temperature - 384) / 64000 * 200 - 50);
            var realBattery = ((float)battery / 1000);

            return new DecentSensorFrame
            {
                DeviceId = deviceId,
                Pressure = realPressure,
                Temperature = realTemperature,
                Battery = realBattery,
                TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(message.ts)
            };
        }

        IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Service...");

            _webSocket.Close(CloseStatusCode.Away, "Service shutting down");

            return Task.CompletedTask;
        }
    }
}
