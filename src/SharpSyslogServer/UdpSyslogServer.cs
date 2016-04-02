using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpSyslogServer.Networking;

namespace SharpSyslogServer
{
    public sealed class UdpSyslogServer : ISyslogServer
    {
        private readonly ISyslogMessageHandler _syslogMessageHandler;
        private readonly Func<IUdpClient> _updClientFactory;
        private readonly Func<DateTime> _nowFunc;

        public UdpSyslogServer(ISyslogMessageHandler syslogMessageHandler)
            : this(syslogMessageHandler, () => new UdpClientAdapter(514), () => DateTime.UtcNow)
        { }

        internal UdpSyslogServer(
            ISyslogMessageHandler syslogMessageHandler,
            Func<IUdpClient> updClientFactory,
            Func<DateTime> nowFunc)
        {
            if (syslogMessageHandler == null) throw new ArgumentNullException(nameof(syslogMessageHandler));
            if (updClientFactory == null) throw new ArgumentNullException(nameof(updClientFactory));
            if (nowFunc == null) throw new ArgumentNullException(nameof(nowFunc));
            _syslogMessageHandler = syslogMessageHandler;
            _updClientFactory = updClientFactory;
            _nowFunc = nowFunc;
        }

        /// <summary>
        /// Starts receiving syslog messages
        /// </summary>
        /// <param name="token">CancellationToken to stop the server</param>
        /// <returns></returns>
        public Task Start(CancellationToken token)
        {
            return Task.Run(async () =>
            {
                using (var udpClient = _updClientFactory())
                {
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();

                        var received = await udpClient.ReceiveAsync().WithCancellation(token).ConfigureAwait(false);
                        var log = Encoding.UTF8.GetString(received.Buffer, 0, received.Buffer.Length);

                        _syslogMessageHandler.Handle(new SyslogMessage(received.RemoteEndPoint, log, _nowFunc()));
                    }
                }
            }, token);
        }
    }
}