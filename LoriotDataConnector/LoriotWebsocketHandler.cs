using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp;

namespace LoriotDataConnector
{
    class LoriotWebsocketHandler
    {
        private WebSocket _socket;
        public event EventHandler<MessageRecievedEventArgs> MessageRecieved;

        public LoriotWebsocketHandler(string token)
        {
            _socket = new WebSocket("wss://iotnet.teracom.dk/app?token=" + token);
            _socket.OnMessage += HandleRecievedMessage;
        }

        public void Connect()
        {
            _socket.Connect();
        }

        private void HandleRecievedMessage(object sender, MessageEventArgs e)
        {
            var command = JsonConvert.DeserializeObject<Command>(e.Data);
            LoriotMessageType messageType; 

            switch (command.cmd)
            {
                case "rx":
                    messageType = LoriotMessageType.UplinkMessage;
                    break;
                case "gw":
                    messageType = LoriotMessageType.GatewayMessage;
                    break;
                case "cq":
                    messageType = LoriotMessageType.CacheMessage;
                    break;
                default:
                    throw new ApplicationException($"Recieved invalid message type: {command.cmd}");
            }

            MessageRecieved?.Invoke(this, new MessageRecievedEventArgs(messageType, e.Data));
        }

        public void Send(string message)
        {
            _socket.Send(message);
        }

        public void Close(CloseStatusCode statusCode, string reason)
        {
            _socket.Close(statusCode,reason);
        }
    }

    public enum LoriotMessageType
    {
        UplinkMessage,
        GatewayMessage,
        CacheMessage
    }
}
