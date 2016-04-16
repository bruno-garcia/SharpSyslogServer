using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;

namespace SharpSyslogServer
{
    public sealed class LoggerRawMessageHandler : IRawMessageHandler
    {
        private readonly IRawMessageHandler _rawMessageHandler;
        private readonly ILogger _logger;

        public LoggerRawMessageHandler(IRawMessageHandler rawMessageHandler, ILogger logger)
        {
            if (rawMessageHandler == null) throw new ArgumentNullException(nameof(rawMessageHandler));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _rawMessageHandler = rawMessageHandler;
            _logger = logger;
        }

        public void Handle(IRawMessage rawMessage)
        {
            try
            {
                _rawMessageHandler.Handle(rawMessage);
            }
            catch (Exception ex)
            {
                var encodedMessage = Convert.ToBase64String(rawMessage.Payload);
                var format = "Failed to process message. Received {receivedAt}, RawMessage: {message}";
                _logger.LogError(new FormattedLogValues(format, rawMessage.ReceivedAt, encodedMessage), ex);
            }
        }
    }
}
