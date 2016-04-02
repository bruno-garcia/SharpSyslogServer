using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SharpSyslogServer.Networking
{
    internal interface IUdpClient : IDisposable
    {
        Task<UdpReceiveResult> ReceiveAsync();
    }
}