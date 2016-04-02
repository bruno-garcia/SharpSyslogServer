using System;
using System.Threading;
using System.Threading.Tasks;
using SharpSyslogServer.Networking;

namespace SharpSyslogServer
{
    public sealed class UdpSyslogServer : ISyslogServer
    {
        private readonly IRawMessageHandler _rawMessageHandler;
        private readonly Func<IUdpClient> _updClientFactory;
        private readonly Func<DateTime> _nowFunc;

        public UdpSyslogServer(IRawMessageHandler rawMessageHandler)
            : this(rawMessageHandler, () => new UdpClientAdapter(514), () => DateTime.UtcNow)
        { }

        internal UdpSyslogServer(
            IRawMessageHandler rawMessageHandler,
            Func<IUdpClient> updClientFactory,
            Func<DateTime> nowFunc)
        {
            if (rawMessageHandler == null) throw new ArgumentNullException(nameof(rawMessageHandler));
            if (updClientFactory == null) throw new ArgumentNullException(nameof(updClientFactory));
            if (nowFunc == null) throw new ArgumentNullException(nameof(nowFunc));
            _rawMessageHandler = rawMessageHandler;
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
                    do
                    {
                        token.ThrowIfCancellationRequested();

                        var received = await udpClient.ReceiveAsync()
                            .WithCancellation(token)
                            .ConfigureAwait(false);

                        _rawMessageHandler.Handle(
                            new RawMessage(received.RemoteEndPoint, received.Buffer, _nowFunc()));

                    } while (!token.IsCancellationRequested);
                }
            }, token);
        }
    }
}