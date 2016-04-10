using System;
using System.Collections.Generic;
using System.Linq;
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
        private const string TimestampPattern = @"\d{4}(-\d{2}){2}(T(\d{2})(:\d{2}){2}(\.[0-9]+)?)?(Z|[+-]\d{2}:\d{2})?";
        private const string StructuredDataElementParameterPattern = @"(?<Parameters>(?<Key>\w+)=""(?<Value>\w+)""\s?)+";
        private const string StructuredDataElementPattern = @"(?<StructuredDataElementId>.+?)\s" + StructuredDataElementParameterPattern;
        internal const string StructuredDataPattern = @"(\[(?<StructuredDataElement>" + StructuredDataElementPattern + @")\])+";

        private const string MessagePattern = @"
^(?<Header>
    <(?<Priority>\d{1,3})>
    (?<Version>\d{1,2})?\s
    (\0|(?<EventTime>" + TimestampPattern + @"))\s
    (\0|(?<Hostname>[^\s]+))\s
    (\0|(?<AppName>[^\s]+))\s
    (\0|(?<ProcessId>[^\s]+))\s
    (\0|(?<MessageId>[^\s]+))\s
)
(\0|" + StructuredDataPattern + @")(\s|$)
(?<Message>.*)$";

        internal Regex SyslogFormatRegex { get; }
        internal Regex StructuredDataElementRegex { get; }
        internal Regex StructuredDataElementKeyValueRegex { get; }
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
            var messageString = Encoding.UTF8.GetString(payload);
            return Parse(messageString);
        }

        internal SyslogMessage Parse(string messageString)
        {
            var match = SyslogFormatRegex.Match(messageString);
            if (!match.Success) throw new InvalidOperationException($"Invalid syslog Message '{messageString}' - It doesn't match Regular Expression: {MessagePattern}");

            var headerGroup = match.Groups["Header"];
            if (!headerGroup.Success) throw new InvalidOperationException("Message doesn't have a Header");

            return new SyslogMessage(
                ParseHeader(match),
                ParseStructuredData(match).ToList(),
                ParseMessage(match)
                );
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
                                            .ToDictionary(k => k.Key, v => v.Value));
        }

        private KeyValuePair<string, string> ParseParameter(string keyValue)
        {
            var keyValueMatch = StructuredDataElementKeyValueRegex.Match(keyValue);
            return new KeyValuePair<string, string>(keyValueMatch.Groups["Key"].Value, keyValueMatch.Groups["Value"].Value);
        }

        private Header ParseHeader(Match match)
        {
            var priorityNumber = match.ParseSuccessfulDouble("Priority");
            if (!priorityNumber.HasValue || priorityNumber < 0 || priorityNumber > 191)
                throw new Exception($"Invalid Priority value: {priorityNumber}");

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

            var message = value?.TrimStart('\uFEFF', '\0');
            return string.IsNullOrWhiteSpace(message)
                ? null
                : message;
        }
    }
}
