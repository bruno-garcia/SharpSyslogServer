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

            AssertEqual(testCase.ExpectedMessage, actualMessage);
        }

        private static void AssertEqual(SyslogMessage expected, SyslogMessage actual)
        {
            //Assert.Equal(expected, actual);

            Assert.Equal(expected.Header, actual.Header);
            Assert.Equal(expected.Message, actual.Message);
            //Assert.Equal(expected.StructuredData, actual.StructuredData);

            //Assert.True(expected.StructuredData.Count == actual.StructuredData.Count && !expected.StructuredData.Except(actual.StructuredData).Any());
            Assert.Equal(expected.Header.Priority, actual.Header.Priority);
            Assert.Equal(expected.Header.Version, actual.Header.Version);
            Assert.Equal(expected.Header.EventTime, actual.Header.EventTime);
            Assert.Equal(expected.Header.Hostname, actual.Header.Hostname);
            Assert.Equal(expected.Header.AppName, actual.Header.AppName);
            Assert.Equal(expected.Header.ProcessId, actual.Header.ProcessId);
            Assert.Equal(expected.Header.MessageId, actual.Header.MessageId);
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

        public static IEnumerable<object[]> GetSampleMessages()
        {
            // \uFEFF = BOM
            yield return new object[] { new SampleMessage // all nulls, absent message
            {
                RawMessage = "<0> \0 \0 \0 \0 \0 \0",
                ExpectedMessage = new SyslogMessage{Header = new Header()}
            }};
            yield return new object[] { new SampleMessage // all nulls
            {
                RawMessage = "<0> \0 \0 \0 \0 \0 \0 \0",
                ExpectedMessage = new SyslogMessage{Header = new Header()}
            }};
            yield return new object[] { new SampleMessage // all nulls but message
            {
                RawMessage = "<0> \0 \0 \0 \0 \0 \0 a",
                ExpectedMessage = new SyslogMessage
                {
                    Header = new Header(),
                    Message = "a"
                }
            }};
            yield return new object[] { new SampleMessage  // single structuredData, no msg
            {
                RawMessage = "<0> \0 \0 \0 \0 \0 [a a=\"b\"]",
                ExpectedMessage = new SyslogMessage
                {
                    Header = new Header(),
                    StructuredData = new[] {
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "a",
                            Parameters = new Dictionary<string, string>{{ "a", "b" }}
                        }
                    }
                }
            }};
            yield return new object[] { new SampleMessage // multiple structuredData, no msg
            {
                RawMessage = "<0> \0 \0 \0 \0 \0 [a a=\"b\" c=\"d\"][b a=\"b\"]",
                ExpectedMessage = new SyslogMessage
                {
                    Header = new Header(),
                    StructuredData = new[] {
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "a",
                            Parameters = new Dictionary<string, string>{ { "a", "b" }, { "c", "d" } }
                        },
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "b",
                            Parameters = new Dictionary<string, string>{ { "a", "b" } }
                        }
                    }
                }
            }};
            yield return new object[] { new SampleMessage // multiple structuredData, with msg
            {
                RawMessage = "<0> \0 \0 \0 \0 \0 [a a=\"b\" c=\"d\"][b a=\"b\"][c a=\"b\"] [msg] [msg]",
                ExpectedMessage = new SyslogMessage
                {
                    Header = new Header(),
                    StructuredData = new[] {
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "a",
                            Parameters = new Dictionary<string, string>{ { "a", "b" }, { "c", "d" } }
                        },
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "b",
                            Parameters = new Dictionary<string, string>{ { "a", "b" } }
                        },
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "c",
                            Parameters = new Dictionary<string, string>{ { "a", "b" } }
                        }
                    },
                    Message = "[msg] [msg]"
                }
            }};
            yield return new object[] { new SampleMessage // StructuredData followed by BOM and end of line
            {
                RawMessage = "<0> \0 \0 \0 \0 \0 [id1 k1=\"v1\"][id2 k2=\"v2\"] \uFEFF",
                ExpectedMessage = new SyslogMessage
                {
                    Header = new Header(),
                    StructuredData = new[] {
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "id1",
                            Parameters = new Dictionary<string, string>{{ "k1", "v1" }}
                        },
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "id2",
                            Parameters = new Dictionary<string, string>{{ "k2", "v2" }}
                        }
                    },
                }
            }};
            yield return new object[] { new SampleMessage
            {
                RawMessage = "<165>1 2003-08-24T05:14:15.999999-12:00 mymachine.example.com evntslog procId ID47 [exampleSDID@32473 iut=\"3\"][examplePriority@32473 class=\"high\"] Some ASCII message with [some breakets], No BOM",
                ExpectedMessage = new SyslogMessage
                {
                    Header = new Header
                    {
                        Version = 1,
                        Priority = new Priority(20,5),
                        EventTime = new DateTimeOffset(2003, 08, 24, 05, 14, 15, TimeSpan.FromHours(-12)).AddTicks(9999990),
                        Hostname = "mymachine.example.com",
                        AppName = "evntslog",
                        ProcessId = "procId",
                        MessageId = "ID47",
                    },
                    StructuredData = new[] {
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "exampleSDID@32473",
                            Parameters = new Dictionary<string, string>
                            {
                                { "iut", "3" },
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
                    },
                    Message = "Some ASCII message with [some breakets], No BOM"
                }
            }};
            yield return new object[] { new SampleMessage
            {
                RawMessage = "<34>1 2003-10-11T22:14:15.003Z mymachine.example.com su \0 ID47 \0 \uFEFF'su root' failed for lonvick on /dev/pts/8",
                ExpectedMessage = new SyslogMessage
                {
                    Header = new Header
                    {
                        Version = 1,
                        Priority = new Priority(4,2),
                        EventTime = new DateTimeOffset(2003, 10, 11, 22, 14, 15, 3, TimeSpan.Zero),
                        Hostname = "mymachine.example.com",
                        AppName = "su",
                        ProcessId = null,
                        MessageId = "ID47",
                    },
                    StructuredData = null,
                    Message = "'su root' failed for lonvick on /dev/pts/8"
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
                        Priority = new Priority(4,2),
                        EventTime = null,
                        Hostname = null,
                        AppName = null,
                        ProcessId = null,
                        MessageId = null,
                    },
                    StructuredData = null,
                    Message = "'su root' failed for lonvick on /dev/pts/8"
                }
            }};
            yield return new object[] { new SampleMessage
            {
                    // As the Unicode BOM is missing, the syslog application does not know the encoding of the MSG part.
                RawMessage = "<165>1 2003-08-24T05:14:15.000003-07:00 192.0.2.1 myproc 8710 \0 \0 %% It's time to make the do-nuts.",
                ExpectedMessage = new SyslogMessage
                {
                    Header = new Header
                    {
                        Version = 1,
                        Priority = new Priority(20,5),
                        EventTime = new DateTimeOffset(2003, 08, 24, 05, 14, 15, TimeSpan.FromHours(-7))
                                            .AddTicks(30),
                        Hostname = "192.0.2.1",
                        AppName = "myproc",
                        ProcessId = "8710",
                        MessageId = null,
                    },
                    StructuredData = null,
                    Message = "%% It's time to make the do-nuts."
                }
            }};
            yield return new object[] { new SampleMessage
            {
                RawMessage = "<165>1 2003-10-11T22:14:15.003Z mymachine.example.com evntslog \0 ID47 [exampleSDID@32473 iut=\"3\" eventSource=\"Application\" eventID=\"1011\"] \uFEFFAn application event log entry...",
                ExpectedMessage = new SyslogMessage
                {
                    Header = new Header
                    {
                        Version = 1,
                        Priority = new Priority(20,5),
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
                        }
                    },
                    Message = "An application event log entry..."
                }
            }};
            yield return new object[] { new SampleMessage
            {
                RawMessage = "<165>1 2003-10-11T22:14:15.003Z mymachine.example.com evntslog \0 ID47 [exampleSDID@32473 iut=\"3\" eventSource=\"Application\" eventID=\"1011\"][examplePriority@32473 class=\"high\"]",
                ExpectedMessage = new SyslogMessage
                {
                    Header = new Header
                    {
                        Version = 1,
                        Priority = new Priority(20,5),
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
                    },
                    Message = null
                }
            }};
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
}
