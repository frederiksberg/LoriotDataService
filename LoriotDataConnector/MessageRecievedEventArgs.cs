using System;
using System.Collections.Generic;
using System.Text;

namespace LoriotDataConnector
{
    class MessageRecievedEventArgs : EventArgs
    {
        public LoriotMessageType MessageType { get; private set; }

        public string Data { get; private set; }

        public MessageRecievedEventArgs(LoriotMessageType messageType, string data)
        {
            MessageType = messageType;
            Data = data;
        }
    }
}
