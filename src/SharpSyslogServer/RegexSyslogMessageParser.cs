using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using SharpSyslogServer.SyslogMessageFormat;

namespace SharpSyslogServer
{
    /// <summary>
    /// Parses a syslog message using Regex
    /// </summary>
    /// <remarks>
    /// Will decode the whole raw message using UTF8 and use Regex to find the fields as per RFC 5424 specs
    /// https://tools.ietf.org/html/rfc5424#section-6
    /// </remarks>
    public class RegexSyslogMessageParser : ISyslogMessageParser
    {
        private const char NullChar = '\u0000';
        internal const string TimestampPattern = @"\d{4}(-\d{2}){2}(T(\d{2})(:\d{2}){2}(\.[0-9]+)?)?(Z|[+-]\d{2}:\d{2})?";
        /// <summary>
        /// </summary>
        private static readonly string MessagePattern = $@"
^
(?<{nameof(SyslogMessage.Header)}>
    <(?<{nameof(Header.Priority)}>\d{{1,3}})>
    (?<{nameof(Header.Version)}>\d{{1,2}})?\s
    (({NullChar}|(?<{nameof(Header.EventTime)}>{TimestampPattern}))\s)?
    (({NullChar}|(?<{nameof(Header.Hostname)}>[a-zA-A-Z'\.]+))\s)?
    (({NullChar}|(?<{nameof(Header.AppName)}>[a-zA-A-Z'\.]+))\s)?
    (({NullChar}|(?<{nameof(Header.ProcessId)}>[a-zA-A-Z'\.]+))\s)?
    (({NullChar}|(?<{nameof(Header.MessageId)}>[a-zA-A-Z'\.]+))\s)?
).*
$";
        internal Regex SyslogFormatRegex { get; }

        public RegexSyslogMessageParser() : this(TimeSpan.FromSeconds(1)) { }

        public RegexSyslogMessageParser(TimeSpan regexTimeout)
        {
            SyslogFormatRegex = new Regex(MessagePattern,
                RegexOptions.Compiled |
                RegexOptions.Singleline |
                RegexOptions.IgnorePatternWhitespace,
                regexTimeout);
        }

        public SyslogMessage Parse(byte[] payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            var messageString = Encoding.UTF8.GetString(payload);
            return Parse(messageString);
        }

        internal SyslogMessage Parse(string messageString)
        {
            var match = SyslogFormatRegex.Match(messageString);
            if (!match.Success)
                throw new InvalidOperationException($"Invalid syslog Message '{messageString}' - It doesn't match Regular Expression: {MessagePattern}");

            var msg = new SyslogMessage();

            var headerGroup = match.Groups[nameof(SyslogMessage.Header)];
            if (headerGroup.Success)
            {
                double priorityNumber = 0;
                var priorityGroup = match.Groups[nameof(Header.Priority)];
                if (priorityGroup.Success && double.TryParse(priorityGroup.Value, out priorityNumber))
                {
                    if (priorityNumber < 0 || priorityNumber > 191)
                        throw new Exception($"Invalid Priority value: {priorityNumber}");
                    var header = new Header();
                    var priority = new Priority
                    {
                        Facility = (Facility)(priorityNumber / 8),
                        Severity = (Severity)(priorityNumber % 8)
                    };

                    header.Priority = priority;
                    msg.Header = header;
                }

                DateTimeOffset eventTime;
                var eventTimeGroup = match.Groups[nameof(Header.EventTime)];
                if (eventTimeGroup.Success && TryParseTimestamp(eventTimeGroup.Value, out eventTime))
                {
                    var header = msg.Header ?? new Header();
                    header.EventTime = eventTime;
                    msg.Header = header;
                }

                byte versionNumber = 0;
                var versionGroup = match.Groups[nameof(Header.Version)];
                if (versionGroup.Success && byte.TryParse(versionGroup.Value, out versionNumber))
                {
                    if (versionNumber == 0) throw new Exception("Invalid Version number: 0");

                    var header = msg.Header ?? new Header();
                    header.Version = versionNumber;
                    msg.Header = header;
                }

                var hostnameGroup = match.Groups[nameof(Header.Hostname)];
                if (hostnameGroup.Success)
                {
                    var header = msg.Header ?? new Header();
                    header.Hostname = hostnameGroup.Value;
                    msg.Header = header;
                }
            }

            return msg;
        }

        internal bool TryParseTimestamp(string timestamp, out DateTimeOffset eventTime)
        {
            return DateTimeOffset.TryParse(
                timestamp,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out eventTime);
        }
    }

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
        }
    }
}
