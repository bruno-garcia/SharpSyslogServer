using System;
using SharpSyslogServer.SyslogMessageFormat;

namespace SharpSyslogServer
{
    public sealed class ParserRawMessageHandler : IRawMessageHandler
    {
        private readonly ISyslogMessageParser _syslogMessageParser;
        private readonly ISyslogMessageHandler _syslogMessageHandler;

        public ParserRawMessageHandler(
            ISyslogMessageParser syslogMessageParser,
            ISyslogMessageHandler syslogMessageHandler)
        {
            if (syslogMessageParser == null) throw new ArgumentNullException(nameof(syslogMessageParser));
            if (syslogMessageHandler == null) throw new ArgumentNullException(nameof(syslogMessageHandler));
            _syslogMessageParser = syslogMessageParser;
            _syslogMessageHandler = syslogMessageHandler;
        }

        public void Handle(IRawMessage rawMessage)
        {
            if (rawMessage == null) throw new ArgumentNullException(nameof(rawMessage));
            if (rawMessage.Payload == null) throw new ArgumentException("Message Payload must be provided", nameof(rawMessage));

            var message = _syslogMessageParser.Parse(rawMessage.Payload);
            _syslogMessageHandler.Handle(message);
        }
    }
}