using System;
using System.Net;

namespace SharpSyslogServer
{
    internal sealed class SyslogMessage : ISyslogMessage
    {
        public IPEndPoint RemoteEndPoint { get; }
        public string Message { get; }
        public DateTime ReceivedAt { get; }

        public SyslogMessage(IPEndPoint remoteEndPoint, string message, DateTime receivedAt)
        {
            if (remoteEndPoint == null) throw new ArgumentNullException(nameof(remoteEndPoint));
            if (message == null) throw new ArgumentNullException(nameof(message));
            RemoteEndPoint = remoteEndPoint;
            Message = message;
            ReceivedAt = receivedAt;
        }
    }
}
