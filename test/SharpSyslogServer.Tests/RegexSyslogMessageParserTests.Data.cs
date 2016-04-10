using System;
using System.Collections.Generic;
using SharpSyslogServer.SyslogMessageFormat;

namespace SharpSyslogServerTests
{
    public partial class RegexSyslogMessageParserTests
    {
        public static IEnumerable<object[]> GetSampleValidMessages()
        {
            // \uFEFF = BOM
            yield return new object[]
            {
                new SampleMessage // all nulls, absent message
                {
                    RawMessage = "<0> \0 \0 \0 \0 \0 \0",
                    ExpectedMessage = new SyslogMessage
                    {
                        Header = new Header(),
                        StructuredData = new StructuredDataElement[0]
                    }
                }
            };
            yield return new object[]
            {
                new SampleMessage // all nulls
                {
                    RawMessage = "<0> \0 \0 \0 \0 \0 \0 \0",
                    ExpectedMessage = new SyslogMessage
                    {
                        Header = new Header(),
                        StructuredData = new StructuredDataElement[0]
                    }
                }
            };
            yield return new object[]
            {
                new SampleMessage // all nulls but message
                {
                    RawMessage = "<0> \0 \0 \0 \0 \0 \0 a",
                    ExpectedMessage = new SyslogMessage
                    {
                        Header = new Header(),
                        Message = "a",
                        StructuredData = new StructuredDataElement[0]
                    }
                }
            };
            yield return new object[]
            {
                new SampleMessage // single structuredData, no msg
                {
                    RawMessage = "<0> \0 \0 \0 \0 \0 [a a=\"b\"]",
                    ExpectedMessage = new SyslogMessage
                    {
                        Header = new Header(),
                        StructuredData = new[]
                        {
                            new StructuredDataElement
                            {
                                StructuredDataElementId = "a",
                                Parameters = new Dictionary<string, string> {{"a", "b"}}
                            }
                        }
                    }
                }
            };
            yield return new object[]
            {
                new SampleMessage // multiple structuredData, no msg
                {
                    RawMessage = "<0> \0 \0 \0 \0 \0 [a a=\"b\" c=\"d\"][b a=\"b\"]",
                    ExpectedMessage = new SyslogMessage
                    {
                        Header = new Header(),
                        StructuredData = new[]
                        {
                            new StructuredDataElement
                            {
                                StructuredDataElementId = "a",
                                Parameters = new Dictionary<string, string> {{"a", "b"}, {"c", "d"}}
                            },
                            new StructuredDataElement
                            {
                                StructuredDataElementId = "b",
                                Parameters = new Dictionary<string, string> {{"a", "b"}}
                            }
                        }
                    }
                }
            };
            yield return new object[]
            {
                new SampleMessage // multiple structuredData, with msg
                {
                    RawMessage = "<0> \0 \0 \0 \0 \0 [a a=\"b\" c=\"d\"][b a=\"b\"][c a=\"b\"] [msg] [msg]",
                    ExpectedMessage = new SyslogMessage
                    {
                        Header = new Header(),
                        StructuredData = new[]
                        {
                            new StructuredDataElement
                            {
                                StructuredDataElementId = "a",
                                Parameters = new Dictionary<string, string> {{"a", "b"}, {"c", "d"}}
                            },
                            new StructuredDataElement
                            {
                                StructuredDataElementId = "b",
                                Parameters = new Dictionary<string, string> {{"a", "b"}}
                            },
                            new StructuredDataElement
                            {
                                StructuredDataElementId = "c",
                                Parameters = new Dictionary<string, string> {{"a", "b"}}
                            }
                        },
                        Message = "[msg] [msg]"
                    }
                }
            };
            yield return new object[]
            {
                new SampleMessage // StructuredData followed by BOM and end of line
                {
                    RawMessage = "<0> \0 \0 \0 \0 \0 [id1 k1=\"v1\"][id2 k2=\"v2\"] \uFEFF",
                    ExpectedMessage = new SyslogMessage
                    {
                        Header = new Header(),
                        StructuredData = new[]
                        {
                            new StructuredDataElement
                            {
                                StructuredDataElementId = "id1",
                                Parameters = new Dictionary<string, string> {{"k1", "v1"}}
                            },
                            new StructuredDataElement
                            {
                                StructuredDataElementId = "id2",
                                Parameters = new Dictionary<string, string> {{"k2", "v2"}}
                            }
                        },
                    }
                }
            };
            yield return new object[]
            {
                new SampleMessage
                {
                    RawMessage =
                        "<165>1 2003-08-24T05:14:15.999999-12:00 mymachine.example.com evntslog procId ID47 [exampleSDID@32473 iut=\"3\"][examplePriority@32473 class=\"high\"] Some ASCII message with [some breakets], No BOM",
                    ExpectedMessage = new SyslogMessage
                    {
                        Header = new Header
                        {
                            Version = 1,
                            Priority = new Priority(20, 5),
                            EventTime =
                                new DateTimeOffset(2003, 08, 24, 05, 14, 15, TimeSpan.FromHours(-12)).AddTicks(9999990),
                            Hostname = "mymachine.example.com",
                            AppName = "evntslog",
                            ProcessId = "procId",
                            MessageId = "ID47",
                        },
                        StructuredData = new[]
                        {
                            new StructuredDataElement
                            {
                                StructuredDataElementId = "exampleSDID@32473",
                                Parameters = new Dictionary<string, string>
                                {
                                    {"iut", "3"},
                                }
                            },
                            new StructuredDataElement
                            {
                                StructuredDataElementId = "examplePriority@32473",
                                Parameters = new Dictionary<string, string>
                                {
                                    {"class", "high"}
                                }
                            },
                        },
                        Message = "Some ASCII message with [some breakets], No BOM"
                    }
                }
            };
            yield return new object[]
            {
                new SampleMessage
                {
                    RawMessage =
                        "<34>1 2003-10-11T22:14:15.003Z mymachine.example.com su \0 ID47 \0 \uFEFF'su root' failed for lonvick on /dev/pts/8",
                    ExpectedMessage = new SyslogMessage
                    {
                        Header = new Header
                        {
                            Version = 1,
                            Priority = new Priority(4, 2),
                            EventTime = new DateTimeOffset(2003, 10, 11, 22, 14, 15, 3, TimeSpan.Zero),
                            Hostname = "mymachine.example.com",
                            AppName = "su",
                            ProcessId = null,
                            MessageId = "ID47",
                        },
                        StructuredData = new StructuredDataElement[0],
                        Message = "'su root' failed for lonvick on /dev/pts/8"
                    }
                }
            };
            yield return new object[]
            {
                new SampleMessage
                {
                    RawMessage = "<34>1 \0 \0 \0 \0 \0 \0 \uFEFF'su root' failed for lonvick on /dev/pts/8",
                    ExpectedMessage = new SyslogMessage
                    {
                        Header = new Header
                        {
                            Version = 1,
                            Priority = new Priority(4, 2),
                            EventTime = null,
                            Hostname = null,
                            AppName = null,
                            ProcessId = null,
                            MessageId = null,
                        },
                        StructuredData = new StructuredDataElement[0],
                        Message = "'su root' failed for lonvick on /dev/pts/8"
                    }
                }
            };
            yield return new object[]
            {
                new SampleMessage
                {
                    // As the Unicode BOM is missing, the syslog application does not know the encoding of the MSG part.
                    RawMessage =
                        "<165>1 2003-08-24T05:14:15.000003-07:00 192.0.2.1 myproc 8710 \0 \0 %% It's time to make the do-nuts.",
                    ExpectedMessage = new SyslogMessage
                    {
                        Header = new Header
                        {
                            Version = 1,
                            Priority = new Priority(20, 5),
                            EventTime = new DateTimeOffset(2003, 08, 24, 05, 14, 15, TimeSpan.FromHours(-7))
                                .AddTicks(30),
                            Hostname = "192.0.2.1",
                            AppName = "myproc",
                            ProcessId = "8710",
                            MessageId = null,
                        },
                        StructuredData = new StructuredDataElement[0],
                        Message = "%% It's time to make the do-nuts."
                    }
                }
            };
            yield return new object[]
            {
                new SampleMessage
                {
                    RawMessage =
                        "<165>1 2003-10-11T22:14:15.003Z mymachine.example.com evntslog \0 ID47 [exampleSDID@32473 iut=\"3\" eventSource=\"Application\" eventID=\"1011\"] \uFEFFAn application event log entry...",
                    ExpectedMessage = new SyslogMessage
                    {
                        Header = new Header
                        {
                            Version = 1,
                            Priority = new Priority(20, 5),
                            EventTime = new DateTimeOffset(2003, 10, 11, 22, 14, 15, 3, TimeSpan.Zero),
                            Hostname = "mymachine.example.com",
                            AppName = "evntslog",
                            ProcessId = null,
                            MessageId = "ID47",
                        },
                        StructuredData = new[]
                        {
                            new StructuredDataElement
                            {
                                StructuredDataElementId = "exampleSDID@32473",
                                Parameters = new Dictionary<string, string>
                                {
                                    {"iut", "3"},
                                    {"eventSource", "Application"},
                                    {"eventID", "1011"},
                                }
                            }
                        },
                        Message = "An application event log entry..."
                    }
                }
            };
            yield return new object[]
            {
                new SampleMessage
                {
                    RawMessage =
                        "<165>1 2003-10-11T22:14:15.003Z mymachine.example.com evntslog \0 ID47 [exampleSDID@32473 iut=\"3\" eventSource=\"Application\" eventID=\"1011\"][examplePriority@32473 class=\"high\"]",
                    ExpectedMessage = new SyslogMessage
                    {
                        Header = new Header
                        {
                            Version = 1,
                            Priority = new Priority(20, 5),
                            EventTime = new DateTimeOffset(2003, 10, 11, 22, 14, 15, 3, TimeSpan.Zero),
                            Hostname = "mymachine.example.com",
                            AppName = "evntslog",
                            ProcessId = null,
                            MessageId = "ID47",
                        },
                        StructuredData = new[]
                        {
                            new StructuredDataElement
                            {
                                StructuredDataElementId = "exampleSDID@32473",
                                Parameters = new Dictionary<string, string>
                                {
                                    {"iut", "3"},
                                    {"eventSource", "Application"},
                                    {"eventID", "1011"},
                                }
                            },
                            new StructuredDataElement
                            {
                                StructuredDataElementId = "examplePriority@32473",
                                Parameters = new Dictionary<string, string>
                                {
                                    {"class", "high"}
                                }
                            },
                        },
                        Message = null
                    }
                }
            };
        }

        public static IEnumerable<object[]> GetSampleValidDateTime()
        {
            yield return new object[]
            {
                new SampleTimestamp(
                    @"2016-04-03T23:10:15.999Z",
                    new DateTimeOffset(2016, 4, 3, 23, 10, 15, 999, TimeSpan.Zero))
            };
            yield return new object[]
            {
                new SampleTimestamp(
                    @"2016-04-03T23:10:15.999+12:00",
                    new DateTimeOffset(2016, 4, 3, 23, 10, 15, 999, TimeSpan.FromHours(+12)))
            };
            yield return new object[]
            {
                new SampleTimestamp(
                    @"2016-04-03T23:10:15.999-12:00",
                    new DateTimeOffset(2016, 4, 3, 23, 10, 15, 999, TimeSpan.FromHours(-12)))
            };
            yield return new object[]
            {
                new SampleTimestamp(
                    @"2016-04-03T23:10:15.999+01:30",
                    new DateTimeOffset(2016, 4, 3, 23, 10, 15, 999, TimeSpan.FromMinutes(90)))
            };
        }

        public static IEnumerable<object[]> GetSampleStructuredData()
        {
            yield return new object[]
            {
                new SampleStructuredData(
                    // StructuredData multiple captures
                    @"[a1 k=""v""]",
                    // apply now: (?<id>
                    // KeyValue: (?<KeyValue> -> gives many captures Split by =
                    new[]
                    {
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "a1",
                            Parameters = new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("k", "v")
                            }
                        }
                    })
            };
            yield return new object[]
            {
                new SampleStructuredData(
                    @"[a1 k1=""v1""][a2 k2=""v2""]",
                    new[]
                    {
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "a1",
                            Parameters = new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("k1", "v1")
                            }
                        },
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "a2",
                            Parameters = new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("k2", "v2")
                            }
                        }
                    })
            };
            yield return new object[]
            {
                new SampleStructuredData(
                    @"[a1 k1=""v1"" k2=""v2""][a2 k3=""v3""]",
                    new[]
                    {
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "a1",
                            Parameters = new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("k1", "v1"),
                                new KeyValuePair<string, string>("k2", "v2")
                            }
                        },
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "a2",
                            Parameters = new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("k3", "v3")
                            }
                        }
                    })
            };
            yield return new object[]
            {
                new SampleStructuredData(
                    @"[a1 k1=""v1"" k2=""v2"" k3=""v3""][a2 k4=""v4"" k5=""v5""][a3 k6=""v6""]",
                    new[]
                    {
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "a1",
                            Parameters = new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("k1", "v1"),
                                new KeyValuePair<string, string>("k2", "v2"),
                                new KeyValuePair<string, string>("k3", "v3")
                            }
                        },
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "a2",
                            Parameters = new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("k4", "v4"),
                                new KeyValuePair<string, string>("k5", "v5")
                            }
                        },
                        new StructuredDataElement
                        {
                            StructuredDataElementId = "a3",
                            Parameters = new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("k6", "v6")
                            }
                        }
                    })
            };
        }
    }

    public class SampleStructuredData
    {
        public string RawStructuredData { get; set; }
        public IReadOnlyCollection<StructuredDataElement> ExpectedStructuredDataElements { get; set; }

        public SampleStructuredData(string rawStructuredData,
            IReadOnlyCollection<StructuredDataElement> expectedStructuredDataElements)
        {
            RawStructuredData = rawStructuredData;
            ExpectedStructuredDataElements = expectedStructuredDataElements;
        }
    }

    public class SampleTimestamp
    {
        public string RawTimestamp { get; }
        public DateTimeOffset ExpectedDateTimeOffset { get; }

        public SampleTimestamp(string rawTimestamp, DateTimeOffset expectedDateTimeOffset)
        {
            RawTimestamp = rawTimestamp;
            ExpectedDateTimeOffset = expectedDateTimeOffset;
        }
    }

    public class SampleMessage
    {
        public string RawMessage { get; set; }
        public SyslogMessage ExpectedMessage { get; set; }
    }
}