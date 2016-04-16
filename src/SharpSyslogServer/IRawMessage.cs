using System;
using System.Net;

namespace SharpSyslogServer
{
    /// <summary>
    /// Raw message received by syslog transport
    /// </summary>
    public interface IRawMessage
    {
        byte[] Payload { get; }
        IPEndPoint RemoteEndPoint { get; }
    }
}