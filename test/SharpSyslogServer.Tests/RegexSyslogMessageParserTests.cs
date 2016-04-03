using System;
using System.Collections.Generic;
using SharpSyslogServer;
using SharpSyslogServer.SyslogMessageFormat;
using Xunit;

namespace SharpSyslogServerTests
{
    public class RegexSyslogMessageParserTests
    {
        [Theory]
        [MemberData(nameof(GetSampleMessages))]
        public void Parse_ValidMessage_ParsesSyslogMessage(SampleMessage testCase)
        {
            var target = new RegexSyslogMessageParser();
            var actualMessage = target.Parse(testCase.RawMessage);
            Assert.Equal(testCase.ExpectedMessage.Header.Priority, actualMessage.Header.Priority);
            Assert.Equal(testCase.ExpectedMessage.Header.Version, actualMessage.Header.Version);
            Assert.Equal(testCase.ExpectedMessage.Header.EventTime, actualMessage.Header.EventTime);

            //Assert.Equal(testCase.ExpectedMessage, actualMessage);
        }

        public static IEnumerable<object[]> GetSampleMessages()
        {
            // \uFEFF = BOM char
            // Examples from the RFC 5424: https://tools.ietf.org/html/rfc5424#section-6.5
            yield return new object[] { new SampleMessage
            {
                RawMessage = "<34>1 2003-10-11T22:14:15.003Z mymachine.example.com su \0 ID47 \0 \uFEFF'su root' failed for lonvick on /dev/pts/8",
                ExpectedMessage = new SyslogMessage
                {
                    Header = new Header
                    {
                        Version = 1,
                        Priority = new Priority
                        {
                            Facility = (Facility)4,
                            Severity = (Severity)2
                        },
                        EventTime = new DateTimeOffset(2003, 10, 11, 22, 14, 15, 3, TimeSpan.Zero),
                        Hostname = "mymachine.example.com",
                        AppName = "su",
                        ProcessId = null,
                        MessageId = "ID47",
                    },
                    Message = "'su root' failed for lonvick...",
                    StructuredData = null
                }
            }};
            yield return new object[] { new SampleMessage
            {
                RawMessage = "<34>1 \0 \0 \0 \0 \0 \0 \uFEFF'su root' failed for lonvick on /dev/pts/8",
                ExpectedMessage = new SyslogMessage
                {
                    Header = new Header
                    {
                        Version = 1,
                        Priority = new Priority
                        {
                            Facility = (Facility)4,
                            Severity = (Severity)2
                        },
                        EventTime = null,
                        Hostname = null,
                        AppName = null,
                        ProcessId = null,
                        MessageId = null,
                    },
                    Message = "'su root' failed for lonvick...",
                    StructuredData = null
                }
            }};
            yield return new object[] { new SampleMessage
            {
                    // As the Unicode BOM is missing, the syslog application does not know the encoding of the MSG part.
                RawMessage = "<165>1 2003-08-24T05:14:15.000003-07:00 192.0.2.1 myproc 8710 - - %% It's time to make the do-nuts.",
                ExpectedMessage = new SyslogMessage
                {
                    Header = new Header
                    {
                        Version = 1,
                        Priority = new Priority
                        {
                            Facility = (Facility)20,
                            Severity = (Severity)5
                        },
                        EventTime = new DateTimeOffset(2003, 08, 24, 05, 14, 15, TimeSpan.FromHours(-7))
                                            .AddTicks(30),
                        Hostname = "192.0.2.1",
                        AppName = "myproc",
                        ProcessId = "8710",
                        MessageId = null,
                    },
                    Message = "%% It's time to make the do-nuts.",
                    StructuredData = null
                }
            }};
            yield return new object[] { new SampleMessage
            {
                RawMessage = "<165>1 2003-10-11T22:14:15.003Z mymachine.example.com evntslog - ID47 [exampleSDID@32473 iut=\"3\" eventSource=\"Application\" eventID=\"1011\"] \uFEFFAn application event log entry...",
                ExpectedMessage = new SyslogMessage
                {
                    Header = new Header
                    {
                        Version = 1,
                        Priority = new Priority
                        {
                            Facility = (Facility)20,
                            Severity = (Severity)5
                        },
                        EventTime = new DateTimeOffset(2003, 10, 11, 22, 14, 15, 3, TimeSpan.Zero),
                        Hostname = "mymachine.example.com",
                        AppName = "evntslog",
                        ProcessId = null,
                        MessageId = "ID47",
                    },
                    Message = "An application event log entry...",
                    StructuredData = new[] {
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "exampleSDID@32473",
                            Parameters = new Dictionary<string, string>
                            {
                                { "iut", "3" },
                                { "eventSource", "Application" },
                                { "eventID", "1011" },
                            }
                        }
                    }
                }
            }};
            yield return new object[] { new SampleMessage
            {
                RawMessage = "<165>1 2003-10-11T22:14:15.003Z mymachine.example.com evntslog - ID47 [exampleSDID@32473 iut=\"3\" eventSource=\"Application\" eventID=\"1011\"][examplePriority@32473 class=\"high\"]",
                ExpectedMessage = new SyslogMessage
                {
                    Header = new Header
                    {
                        Version = 1,
                        Priority = new Priority
                        {
                            Facility = (Facility)20,
                            Severity = (Severity)5
                        },
                        EventTime = new DateTimeOffset(2003, 10, 11, 22, 14, 15, 3, TimeSpan.Zero),
                        Hostname = "mymachine.example.com",
                        AppName = "evntslog",
                        ProcessId = null,
                        MessageId = "ID47",
                    },
                    StructuredData = new[] {
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "exampleSDID@32473",
                            Parameters = new Dictionary<string, string>
                            {
                                { "iut", "3" },
                                { "eventSource", "Application" },
                                { "eventID", "1011" },
                            }
                        },
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "examplePriority@32473",
                            Parameters = new Dictionary<string, string>
                            {
                                { "class", "high" }
                            }
                        },
                    }
                }
            }};
        }

        [Theory]
        [MemberData(nameof(GetSampleDateTime))]
        public void Parse_DateTime_AsExpected(SampleDateTime testCase)
        {
            var target = new RegexSyslogMessageParser();
            DateTimeOffset actual;
            Assert.True(target.TryParseTimestamp(testCase.Sample, out actual));
            Assert.Equal(testCase.Expected, actual);
        }

        public static IEnumerable<object[]> GetSampleDateTime()
        {
            yield return new object[] {
                new SampleDateTime(
                    @"2016-04-03T23:10:15.999Z",
                    new DateTimeOffset(2016,4,3,23,10,15,999, TimeSpan.Zero))
            };
            yield return new object[] {
                new SampleDateTime(
                    @"2016-04-03T23:10:15.999+12:00",
                    new DateTimeOffset(2016,4,3,23,10,15,999, TimeSpan.FromHours(+12)))
            };
            yield return new object[] {
                new SampleDateTime(
                    @"2016-04-03T23:10:15.999-12:00",
                    new DateTimeOffset(2016,4,3,23,10,15,999, TimeSpan.FromHours(-12)))
            };
            yield return new object[] {
                new SampleDateTime(
                    @"2016-04-03T23:10:15.999+01:30",
                    new DateTimeOffset(2016,4,3,23,10,15,999,TimeSpan.FromMinutes(90)))
            };
        }

    }

    public class SampleDateTime
    {
        public string Sample { get; }
        public DateTimeOffset Expected { get; }

        public SampleDateTime(string sample, DateTimeOffset expected)
        {
            Sample = sample;
            Expected = expected;
        }
    }



    public class SampleMessage
    {
        public string RawMessage { get; set; }
        public SyslogMessage ExpectedMessage { get; set; }
    }
}
