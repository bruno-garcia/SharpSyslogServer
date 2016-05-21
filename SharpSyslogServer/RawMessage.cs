using System;
using System.Net;

namespace SharpSyslogServer
{
    internal struct RawMessage : IRawMessage
    {
        public IPEndPoint RemoteEndPoint { get; }
        public byte[] Payload { get; }

        public RawMessage(IPEndPoint remoteEndPoint, byte[] payload)
        {
            if (remoteEndPoint == null) throw new ArgumentNullException(nameof(remoteEndPoint));
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            RemoteEndPoint = remoteEndPoint;
            Payload = payload;
        }
    }
}
