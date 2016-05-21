using System;
using System.Threading;
using System.Threading.Tasks;
using SharpSyslogServer.Networking;

namespace SharpSyslogServer.Transport
{
    public sealed class UdpSyslogServer : ISyslogServer
    {
        private readonly IRawMessageHandler _rawMessageHandler;
        private readonly Func<IUdpClient> _updClientFactory;

        public UdpSyslogServer(IRawMessageHandler rawMessageHandler)
            : this(rawMessageHandler, () => new UdpClientAdapter(514))
        { }

        internal UdpSyslogServer(
            IRawMessageHandler rawMessageHandler,
            Func<IUdpClient> updClientFactory)
        {
            if (rawMessageHandler == null) throw new ArgumentNullException(nameof(rawMessageHandler));
            if (updClientFactory == null) throw new ArgumentNullException(nameof(updClientFactory));
            _rawMessageHandler = rawMessageHandler;
            _updClientFactory = updClientFactory;
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
                    while (!token.IsCancellationRequested)
                    {
                        var received = await udpClient.ReceiveAsync()
                            .WithCancellation(token)
                            .ConfigureAwait(false);

                        _rawMessageHandler.Handle(
                            new RawMessage(received.RemoteEndPoint, received.Buffer));
                    }
                    token.ThrowIfCancellationRequested();
                }
            }, token);
        }
    }
}