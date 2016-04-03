using System;
using System.Collections.Generic;
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
        private const char ByteOrderMark = '\uFEFF';

        internal const string TimestampPattern = @"\d{4}(-\d{2}){2}(T(\d{2})(:\d{2}){2}(\.[0-9]+)?)?(Z|[+-]\d{2}:\d{2})?";

        private static readonly string MessagePattern = $@"
^(?<{nameof(SyslogMessage.Header)}>
    <(?<{nameof(Header.Priority)}>\d{{1,3}})>
    (?<{nameof(Header.Version)}>\d{{1,2}})?\s
    ({NullChar}|(?<{nameof(Header.EventTime)}>{TimestampPattern}))\s
    ({NullChar}|(?<{nameof(Header.Hostname)}>[^\s]+))\s
    ({NullChar}|(?<{nameof(Header.AppName)}>[^\s]+))\s
    ({NullChar}|(?<{nameof(Header.ProcessId)}>[^\s]+))\s
    ({NullChar}|(?<{nameof(Header.MessageId)}>[^\s]+))\s
)
({NullChar}|(?<{nameof(SyslogMessage.StructuredData)}>(\[.*\])))\s
(?<{nameof(SyslogMessage.Message)}>.*)";

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
            if (!match.Success) throw new InvalidOperationException($"Invalid syslog Message '{messageString}' - It doesn't match Regular Expression: {MessagePattern}");

            var headerGroup = match.Groups[nameof(SyslogMessage.Header)];
            if (!headerGroup.Success) throw new InvalidOperationException("Message doesn't have a Header");

            return new SyslogMessage
            {
                Header = ParseHeader(match),
                StructuredData = ParseStructuredData(match),
                Message = ParseMessage(match)
            };
        }

        private string ParseMessage(Match match)
        {
            string message = null;
            ParseSuccessGroup(match.Groups[nameof(SyslogMessage.Message)], s => message = s);
            message = message?.TrimStart(ByteOrderMark, NullChar);
            return string.IsNullOrWhiteSpace(message) 
                ? null 
                : message;
        }

        private IReadOnlyCollection<StructuredDataElement> ParseStructuredData(Match match)
        {
            return null;
        }

        private Header ParseHeader(Match match)
        {
            var header = new Header();

            ParseSuccessGroup(match.Groups[nameof(Header.Priority)],
                (double priorityNumber) =>
                {
                    if (priorityNumber < 0 || priorityNumber > 191)
                        throw new Exception($"Invalid Priority value: {priorityNumber}");

                    header.Priority = new Priority(
                        (Facility)(priorityNumber / 8),
                        (Severity)(priorityNumber % 8));
                });

            ParseSuccessGroup(match.Groups[nameof(Header.EventTime)], s => header.EventTime = s);
            ParseSuccessGroup(match.Groups[nameof(Header.Version)], s => header.Version = s);
            ParseSuccessGroup(match.Groups[nameof(Header.Hostname)], s => header.Hostname = s);
            ParseSuccessGroup(match.Groups[nameof(Header.AppName)], s => header.AppName = s);
            ParseSuccessGroup(match.Groups[nameof(Header.MessageId)], s => header.MessageId = s);
            ParseSuccessGroup(match.Groups[nameof(Header.ProcessId)], s => header.ProcessId = s);

            return header;
        }

        private void ParseSuccessGroup(Group group, Action<DateTimeOffset> setValueCallback)
        {
            DateTimeOffset dateTimeOffset;
            if (group.Success && TryParseTimestamp(group.Value, out dateTimeOffset))
                setValueCallback(dateTimeOffset);
        }

        private void ParseSuccessGroup(Group group, Action<byte> setValueCallback)
        {
            byte @byte;
            if (group.Success && byte.TryParse(group.Value, out @byte))
                setValueCallback(@byte);
        }

        private void ParseSuccessGroup(Group group, Action<double> setValueCallback)
        {
            double @double;
            if (group.Success && double.TryParse(group.Value, out @double))
                setValueCallback(@double);
        }

        private void ParseSuccessGroup(Group group, Action<string> setValueCallback)
        {
            if (group.Success)
                setValueCallback(group.Value);
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
}
