using System;
using System.Net;

namespace SharpSyslogServer
{
    public interface ISyslogMessage
    {
        string Message { get; }
        DateTime ReceivedAt { get; }
        IPEndPoint RemoteEndPoint { get; }
    }
}