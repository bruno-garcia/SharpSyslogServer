using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SharpSyslogServer;
using SharpSyslogServer.SyslogMessageFormat;
using Xunit;

namespace SharpSyslogServerTests
{
    public sealed partial class RegexSyslogMessageParserTests
    {
        [Theory]
        [MemberData(nameof(GetSampleValidMessages))]
        public void Parse_ValidMessage_ParsesSyslogMessage(SampleMessage testCase)
        {
            var target = new RegexSyslogMessageParser();
            var actualMessage = target.Parse(testCase.GetMessageBytes());

            AssertEqual(testCase.ExpectedMessage, actualMessage);
        }

        [Theory]
        [MemberData(nameof(GetSampleValidMessages))]
        public void TryParse_ValidMessage_ParsesSyslogMessage(SampleMessage testCase)
        {
            var target = new RegexSyslogMessageParser();
            SyslogMessage actualMessage;

            Assert.True(target.TryParse(testCase.GetMessageBytes(), out actualMessage));

            AssertEqual(testCase.ExpectedMessage, actualMessage);
        }

        [Fact]
        public void Parse_InvalidMessage_ThrowsException()
        {
            var target = new RegexSyslogMessageParser();
            Assert.Throws<InvalidOperationException>(() => target.Parse(Encoding.UTF8.GetBytes("Not a valid message")));
        }

        [Fact]
        public void TryParse_InvalidMessage_ReturnsFalse()
        {
            var target = new RegexSyslogMessageParser();

            SyslogMessage syslogMessage;
            Assert.False(target.TryParse(Encoding.UTF8.GetBytes("Not a valid message"), out syslogMessage));
            Assert.Null(syslogMessage);
        }

        [Fact]
        public void TryParse_NullMessage_ReturnsFalse()
        {
            var target = new RegexSyslogMessageParser();

            SyslogMessage syslogMessage;
            Assert.False(target.TryParse(null, out syslogMessage));
            Assert.Null(syslogMessage);
        }

        [Theory]
        [MemberData(nameof(GetSampleStructuredData))]
        public void Parse_ValidStructuredData_ParsesStructuredDataElement(SampleStructuredData testCase)
        {
            var expected = testCase.ExpectedStructuredDataElements;
            var target = new RegexSyslogMessageParser();
            var regex = new Regex(RegexSyslogMessageParser.StructuredDataPattern, RegexSyslogMessageParser.Flags, TimeSpan.FromSeconds(1));
            var match = regex.Match(testCase.RawStructuredData);
            var actual = target.ParseStructuredData(match).ToList();

            AssertEqual(expected, actual);
        }

        private static void AssertEqual(SyslogMessage expected, SyslogMessage actual)
        {
            Assert.Equal(expected.Message, actual.Message);

            Assert.Equal(expected.Header.Priority, actual.Header.Priority);
            Assert.Equal(expected.Header.Version, actual.Header.Version);
            Assert.Equal(expected.Header.EventTime, actual.Header.EventTime);
            Assert.Equal(expected.Header.Hostname, actual.Header.Hostname);
            Assert.Equal(expected.Header.AppName, actual.Header.AppName);
            Assert.Equal(expected.Header.ProcessId, actual.Header.ProcessId);
            Assert.Equal(expected.Header.MessageId, actual.Header.MessageId);

            Assert.Equal(expected.Header, actual.Header); // if every header value was equal: header should be equal

            AssertEqual(expected.StructuredData, actual.StructuredData);

            Assert.Equal(expected, actual); // If everything was equal: whole Message should be equal
        }

        private static void AssertEqual(
            IReadOnlyCollection<StructuredDataElement> expected,
            IReadOnlyCollection<StructuredDataElement> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                var expectedItem = expected.ElementAt(i);
                var actualItem = actual.ElementAt(i);

                Assert.Equal(expectedItem.StructuredDataElementId, actualItem.StructuredDataElementId);
                Assert.Equal(expectedItem.Parameters.Count, actualItem.Parameters.Count);

                for (int j = 0; j < expectedItem.Parameters.Count; j++)
                {
                    Assert.Equal(expectedItem.Parameters.ElementAt(j).Key, actualItem.Parameters.ElementAt(j).Key);
                    Assert.Equal(expectedItem.Parameters.ElementAt(j).Value, actualItem.Parameters.ElementAt(j).Value);
                }
            }
        }
    }
}
