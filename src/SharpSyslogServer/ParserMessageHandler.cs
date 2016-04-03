using System;

namespace SharpSyslogServer
{
    public class ParserMessageHandler : IRawMessageHandler
    {
        private readonly ISyslogMessageParser _syslogMessageParser;

        public ParserMessageHandler(ISyslogMessageParser syslogMessageParser)
        {
            if (syslogMessageParser == null) throw new ArgumentNullException(nameof(syslogMessageParser));
            _syslogMessageParser = syslogMessageParser;
        }

        public void Handle(IRawMessage rawMessage)
        {
            if (rawMessage == null) throw new ArgumentNullException(nameof(rawMessage));
            if (rawMessage.Payload == null) throw new ArgumentException("Message Payload must be provided", nameof(rawMessage));

            var message = _syslogMessageParser.Parse(rawMessage.Payload);
            throw new NotImplementedException();
        }
    }
}