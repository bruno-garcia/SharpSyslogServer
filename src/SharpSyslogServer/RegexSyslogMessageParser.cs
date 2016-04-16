using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SharpSyslogServer.SyslogMessageFormat;

namespace SharpSyslogServer
{
    /// <summary>
    /// Regex based RFC 5424 Syslog Message Parser
    /// </summary>
    /// <remarks>
    /// Will decode the whole raw message using UTF8 and use Regex groups to extract the fields as per RFC 5424 specs
    /// https://tools.ietf.org/html/rfc5424#section-6
    /// </remarks>
    public sealed class RegexSyslogMessageParser : ISyslogMessageParser
    {
        private const string TimestampPattern = @"\d{4}(-\d{2}){2}(T(\d{2})(:\d{2}){2}(\.[0-9]+)?)?(Z|[+-]\d{2}:\d{2})?";
        private const string StructuredDataElementParameterPattern = @"(?<Parameters>(?<Key>\w+)=""(?<Value>\w+)""\s?)+";
        private const string StructuredDataElementPattern = @"(?<StructuredDataElementId>.+?)\s" + StructuredDataElementParameterPattern;
        internal const string StructuredDataPattern = @"(\[(?<StructuredDataElement>" + StructuredDataElementPattern + @")\])+";

        private const string MessagePattern = @"
^(?<Header>
    <(?<Priority>\d{1,3})>
    (?<Version>\d{1,2})?\s
    (-|(?<EventTime>" + TimestampPattern + @"))\s
    (-|(?<Hostname>[^\s]+))\s
    (-|(?<AppName>[^\s]+))\s
    (-|(?<ProcessId>[^\s]+))\s
    (-|(?<MessageId>[^\s]+))\s
)
(-|" + StructuredDataPattern + @")(\s|$)
(?<Message>.*)$";

        private Regex SyslogFormatRegex { get; }
        private Regex StructuredDataElementRegex { get; }
        private Regex StructuredDataElementKeyValueRegex { get; }
        private readonly Func<byte[], string> _decoder = Encoding.UTF8.GetString;

        internal const RegexOptions Flags = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace;

        public RegexSyslogMessageParser() : this(TimeSpan.FromSeconds(1)) { }

        public RegexSyslogMessageParser(TimeSpan regexTimeout)
        {
            SyslogFormatRegex = new Regex(MessagePattern, Flags, regexTimeout);
            StructuredDataElementRegex = new Regex(StructuredDataElementPattern, Flags, regexTimeout);
            StructuredDataElementKeyValueRegex = new Regex(StructuredDataElementParameterPattern, Flags, regexTimeout);
        }

        public SyslogMessage Parse(byte[] payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            SyslogMessage syslogMessage = null;
            string messageString = null;

            var parses = TryParse(payload, ref syslogMessage, ref messageString);
            if (!parses || syslogMessage == null)
                throw new InvalidOperationException($"Payload of length: '{payload.Length}' is not a valid RFC 5424 syslog message. UTF8 string: '{messageString}'");

            return syslogMessage;
        }

        public bool TryParse(byte[] payload, out SyslogMessage syslogMessage)
        {
            syslogMessage = null;
            if (payload == null)
                return false;

            string messageString = null;
            return TryParse(payload, ref syslogMessage, ref messageString);
        }

        private bool TryParse(byte[] payload, ref SyslogMessage syslogMessage, ref string messageString)
        {
            if (payload == null)
                return false;

            messageString = _decoder(payload);
            var matchMessage = SyslogFormatRegex.Match(messageString);
            if (!matchMessage.Success)
                return false;

            syslogMessage = Parse(matchMessage, false);
            return true;
        }

        private SyslogMessage Parse(Match matchMessage, bool throwOnInvalidPriority)
        {
            return new SyslogMessage(
                ParseHeader(matchMessage, throwOnInvalidPriority),
                ParseStructuredData(matchMessage).ToList(),
                ParseMessage(matchMessage));
        }

        internal IEnumerable<StructuredDataElement> ParseStructuredData(Match messageMatch)
        {
            var m = messageMatch.Groups["StructuredDataElement"];
            if (!m.Success)
                yield break;

            foreach (var capture in m.Captures)
            {
                var element = ParseStructuredDataElement(capture.ToString());
                if (element != null)
                    yield return element;
            }
        }

        private StructuredDataElement ParseStructuredDataElement(string structuredDataElement)
        {
            var elementMatch = StructuredDataElementRegex.Match(structuredDataElement);
            if (!elementMatch.Success)
                return null;

            return new StructuredDataElement(
                elementMatch.Groups["StructuredDataElementId"].Value,
                elementMatch.Groups["Parameters"].Captures
                    .Cast<Capture>()
                    .Select(c => ParseParameter(c.ToString()))
                    .ToList());
        }

        private KeyValuePair<string, string> ParseParameter(string keyValue)
        {
            var keyValueMatch = StructuredDataElementKeyValueRegex.Match(keyValue);
            return new KeyValuePair<string, string>(keyValueMatch.Groups["Key"].Value, keyValueMatch.Groups["Value"].Value);
        }

        private Header ParseHeader(Match match, bool throwOnInvalidPriority)
        {
            var priorityNumber = match.ParseSuccessfulDouble("Priority");
            if (!priorityNumber.HasValue || priorityNumber < 0 || priorityNumber > 191)
            {
                if (throwOnInvalidPriority)
                    throw new InvalidOperationException($"Invalid Priority value: '{priorityNumber}'");

                return new Header();
            }

            return new Header(
                new Priority(
                     (Facility)(priorityNumber / 8),
                     (Severity)(priorityNumber % 8)),
                match.ParseSuccessfulByte("Version"),
                match.ParseTimestamp("EventTime"),
                match.GroupSuccessValue("Hostname"),
                match.GroupSuccessValue("AppName"),
                match.GroupSuccessValue("ProcessId"),
                match.GroupSuccessValue("MessageId"));
        }

        private string ParseMessage(Match match)
        {
            var group = match.Groups["Message"];
            if (!group.Success) return null;

            var value = match.GroupSuccessValue("Message");

            var message = value?.TrimStart('\uFEFF', '-');
            return string.IsNullOrWhiteSpace(message)
                ? null
                : message;
        }
    }
}
