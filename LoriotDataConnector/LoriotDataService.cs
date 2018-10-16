using GeoAPI.Geometries;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace LoriotWebsocketClient
{
    class LoriotDataService : IHostedService
    {
        private Microsoft.Extensions.Logging.ILogger _logger;
        private DataContext _db;
        private LoriotWebsocketHandler _webSocket;

        public LoriotDataService(ILogger<LoriotDataService> logger, DataContext context, LoriotWebsocketHandler websocketHandler)
        {
            _logger = logger;
            _db = context;
            _webSocket = websocketHandler;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Service...");

            _webSocket.MessageRecieved += OnMessageReceived;

            _webSocket.Connect();

            _logger.LogInformation("Service connected to websocket");

            //_webSocket.Send("{\"cmd\":\"cq\"}");

            return Task.CompletedTask;
        }

        void OnMessageReceived(object sender, MessageRecievedEventArgs e)
        {

            _logger.LogInformation($"Received Message: {e.MessageType}");

            switch (e.MessageType)
            {
                case LoriotMessageType.UplinkMessage:
                    HandleUplinkMessage(e);
                    break;
                case LoriotMessageType.GatewayMessage:
                    HandleGatewayInformation(e);
                    break;
                case LoriotMessageType.CacheMessage:
                    HandleCacheMessage(e);
                    break;
                default:
                    break;
            }

        }

        private void HandleCacheMessage(MessageRecievedEventArgs e)
        {
            var cache = JsonConvert.DeserializeObject<CacheMessage>(e.Data)
                .cache
                .Select(x => DecodeFrame(x.data,x.ts));

            _db.AddRange(cache);
            _db.SaveChanges();
        }

        private void HandleGatewayInformation(MessageRecievedEventArgs e)
        {
            var message = JsonConvert.DeserializeObject<GatewayInformationFrame>(e.Data);

            var factory = new GeometryFactory(new PrecisionModel(PrecisionModels.Floating), 4326);

            var gatewayInfo = message.gws.Select(x => new GatewayInformation
            {
                GatewayId = x.gweui,
                Rssi = x.rssi,
                Snr = x.rssi,
                Location = new Point(new CoordinateArraySequence(new Coordinate[] { new Coordinate(x.lat, x.lon) }),factory)
            });

            _db.GatewayInformation.AddRange(gatewayInfo);
            _db.SaveChanges();
        }

        private void HandleUplinkMessage(MessageRecievedEventArgs e)
        {
            var uplinkMessage = JsonConvert.DeserializeObject<UplinkMessage>(e.Data);
            var mesg = DecodeFrame(uplinkMessage.data,uplinkMessage.ts);
            _db.DecentFrames.Add(mesg);
            _db.SaveChanges();
        }

        WaterLevel DecodeFrame(string data,long timestamp)
        {
            string[] hexValuesSplit = Split(data, 2).ToArray();

            var version = Convert.ToInt32(hexValuesSplit[0], 16);
            var deviceId = Convert.ToInt32(hexValuesSplit[1] + hexValuesSplit[2], 16);
            var flags = Convert.ToInt32(hexValuesSplit[3] + hexValuesSplit[4], 16);
            var pressure = Convert.ToInt32(hexValuesSplit[5] + hexValuesSplit[6], 16);
            var temperature = Convert.ToInt32(hexValuesSplit[7] + hexValuesSplit[8], 16);
            var battery = Convert.ToInt32(hexValuesSplit[9] + hexValuesSplit[10], 16);

            var realPressure = ((float)pressure - 16384) / 32768 * (1 - 0) + 0;
            var realTemperature = (((float)temperature - 384) / 64000 * 200 - 50);
            var realBattery = ((float)battery / 1000);

            return new WaterLevel
            {
                DeviceId = deviceId,
                Pressure = realPressure,
                Temperature = realTemperature,
                Battery = realBattery,
                TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(timestamp)
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
