using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpSyslogServer
{
    public sealed class UdpSyslogServer : ISyslogServer
    {
        private readonly ISyslogMessageHandler _syslogMessageHandler;
        private readonly Func<UdpClient> _updClientFactory;

        public UdpSyslogServer(ISyslogMessageHandler syslogMessageHandler, Func<UdpClient> updClientFactory = null)
        {
            if (syslogMessageHandler == null) throw new ArgumentNullException(nameof(syslogMessageHandler));
            _syslogMessageHandler = syslogMessageHandler;
            _updClientFactory = updClientFactory ?? (() => new UdpClient(514));
        }

        public async Task Start(CancellationToken token)
        {
            using (var udpClient = _updClientFactory())
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    var received = await udpClient.ReceiveAsync().WithCancellation(token).ConfigureAwait(false);
                    var log = Encoding.UTF8.GetString(received.Buffer, 0, received.Buffer.Length);

                    _syslogMessageHandler.Handle(new SyslogMessage(received.RemoteEndPoint, log, DateTime.UtcNow));
                }
            }
        }
    }
}