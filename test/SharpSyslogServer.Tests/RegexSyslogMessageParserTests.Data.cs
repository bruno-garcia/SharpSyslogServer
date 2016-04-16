using System;
using System.Collections.Generic;
using System.Text;
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
                    RawMessage = "<0> - - - - - -",
                    ExpectedMessage = new SyslogMessage()
                }
            };
            yield return new object[]
            {
                new SampleMessage // all nulls
                {
                    RawMessage = "<0> - - - - - - -",
                    ExpectedMessage = new SyslogMessage()
                }
            };
            yield return new object[]
            {
                new SampleMessage // all nulls but message
                {
                    RawMessage = "<0> - - - - - - a",
                    ExpectedMessage = new SyslogMessage("a")
                }
            };
            yield return new object[]
            {
                new SampleMessage // single structuredData, no msg
                {
                    RawMessage = "<0> - - - - - [a a=\"b\"]",
                    ExpectedMessage = new SyslogMessage(new[]
                        {
                            new StructuredDataElement(
                                "a",
                                new Dictionary<string, string> {{"a", "b"}}
                            )
                        })
                }
            };
            yield return new object[]
            {
                new SampleMessage // multiple structuredData, no msg
                {
                    RawMessage = "<0> - - - - - [a a=\"b\" c=\"d\"][b a=\"b\"]",
                    ExpectedMessage = new SyslogMessage(new[]
                        {
                            new StructuredDataElement("a", new Dictionary<string, string> {{"a", "b"}, {"c", "d"}}),
                            new StructuredDataElement("b", new Dictionary<string, string> {{"a", "b"}})
                        })
                }
            };
            yield return new object[]
            {
                new SampleMessage // multiple structuredData, with msg
                {
                    RawMessage = "<0> - - - - - [a a=\"b\" c=\"d\"][b a=\"b\"][c a=\"b\"] [msg] [msg]",
                    ExpectedMessage = new SyslogMessage(
                        "[msg] [msg]",
                        new[]
                        {
                            new StructuredDataElement("a",new Dictionary<string, string> {{"a", "b"}, {"c", "d"}}),
                            new StructuredDataElement("b",new Dictionary<string, string> {{"a", "b"}}),
                            new StructuredDataElement("c",new Dictionary<string, string> {{"a", "b"}})
                        })
                }
            };
            yield return new object[]
            {
                new SampleMessage // StructuredData followed by BOM and end of line
                {
                    RawMessage = "<0> - - - - - [id1 k1=\"v1\"][id2 k2=\"v2\"] \uFEFF",
                    ExpectedMessage = new SyslogMessage(new[]
                        {
                            new StructuredDataElement("id1", new Dictionary<string, string> {{"k1", "v1"}}),
                            new StructuredDataElement("id2", new Dictionary<string, string> {{"k2", "v2"}})
                        })
                }
    };
            yield return new object[]
                    {
                new SampleMessage
                {
                    RawMessage =
                        "<165>1 2003-08-24T05:14:15.999999-12:00 mymachine.example.com evntslog procId ID47 [exampleSDID@32473 iut=\"3\"][examplePriority@32473 class=\"high\"] Some ASCII message with [some breakets], No BOM",
                    ExpectedMessage = new SyslogMessage(
                        new Header(
                            new Priority(20, 5),
                            1,
                            new DateTimeOffset(2003, 08, 24, 05, 14, 15, TimeSpan.FromHours(-12)).AddTicks(9999990),
                            "mymachine.example.com",
                            "evntslog",
                            "procId",
                            "ID47"
                        ),
                        new[]
                        {
                            new StructuredDataElement("exampleSDID@32473",
                                new Dictionary<string, string>
                                {
                                    {"iut", "3"},
                                }
                            ),
                            new StructuredDataElement("examplePriority@32473",
                                new Dictionary<string, string>
                                {
                                    {"class", "high"}
                                }
                            ),
                        },
                        "Some ASCII message with [some breakets], No BOM")
                }
                    };
            yield return new object[]
            {
                new SampleMessage
                {
                    RawMessage =
                        "<34>1 2003-10-11T22:14:15.003Z mymachine.example.com su - ID47 - \uFEFF'su root' failed for lonvick on /dev/pts/8",
                    ExpectedMessage = new SyslogMessage(
                         new Header(
                             new Priority(4, 2),
                             1,
                             new DateTimeOffset(2003, 10, 11, 22, 14, 15, 3, TimeSpan.Zero),
                             "mymachine.example.com",
                             "su",
                             null,
                             "ID47"),
                        message: "'su root' failed for lonvick on /dev/pts/8"
                    )
                }
            };
            yield return new object[]
            {
                new SampleMessage
                {
                    RawMessage = "<34>1 - - - - - - \uFEFF'su root' failed for lonvick on /dev/pts/8",
                    ExpectedMessage = new SyslogMessage(
                         new Header(new Priority(4, 2), 1),
                         message:"'su root' failed for lonvick on /dev/pts/8")
                }
            };
            yield return new object[]
                    {
                new SampleMessage
                {
                    // As the Unicode BOM is missing, the syslog application does not know the encoding of the MSG part.
                    RawMessage =
                        "<165>1 2003-08-24T05:14:15.000003-07:00 192.0.2.1 myproc 8710 - - %% It's time to make the do-nuts.",
                    ExpectedMessage = new SyslogMessage(
                     new Header(
                         new Priority(20, 5),
                         1,
                         new DateTimeOffset(2003, 08, 24, 05, 14, 15, TimeSpan.FromHours(-7)).AddTicks(30),
                        "192.0.2.1",
                        "myproc",
                        "8710",
                        null
                        ),
                        message: "%% It's time to make the do-nuts."
                    )
                }
            };
            yield return new object[]
            {
                new SampleMessage
                {
                    RawMessage =
                        "<165>1 2003-10-11T22:14:15.003Z mymachine.example.com evntslog - ID47 [exampleSDID@32473 iut=\"3\" eventSource=\"Application\" eventID=\"1011\"] \uFEFFAn application event log entry...",
                    ExpectedMessage = new SyslogMessage(
                        new Header(
                            new Priority(20, 5),
                            1,
                            new DateTimeOffset(2003, 10, 11, 22, 14, 15, 3, TimeSpan.Zero),
                            "mymachine.example.com",
                            "evntslog",
                            messageId: "ID47"
                        ),
                        new[]
                        {
                            new StructuredDataElement("exampleSDID@32473",
                                new Dictionary<string, string>
                                {
                                    {"iut", "3"},
                                    {"eventSource", "Application"},
                                    {"eventID", "1011"},
                                }
                            )
                        },
                        "An application event log entry..."
                    )
                }
            };
            yield return new object[]
            {
                new SampleMessage
                {
                    RawMessage =
                        "<165>1 2003-10-11T22:14:15.003Z mymachine.example.com evntslog - ID47 [exampleSDID@32473 iut=\"3\" eventSource=\"Application\" eventID=\"1011\"][examplePriority@32473 class=\"high\"]",
                    ExpectedMessage = new SyslogMessage
                        (new Header(
                             new Priority(20, 5),
                            1,
                            new DateTimeOffset(2003, 10, 11, 22, 14, 15, 3, TimeSpan.Zero),
                            "mymachine.example.com",
                            "evntslog",
                            messageId: "ID47"
                        ),
                        new[]
                        {
                            new StructuredDataElement("exampleSDID@32473",
                                new Dictionary<string, string>
                                {
                                    {"iut", "3"},
                                    {"eventSource", "Application"},
                                    {"eventID", "1011"},
                                }
                            ),
                            new StructuredDataElement("examplePriority@32473",
                                new Dictionary<string, string>
                                {
                                    {"class", "high"}
                                }
                            ),
                        })
                }
            };
        }

        public static IEnumerable<object[]> GetSampleStructuredData()
        {
            yield return new object[]
            {
                new SampleStructuredData(
                    @"[a1 k=""v""]",
                    new[]
                    {
                        new StructuredDataElement("a1",
                            new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("k", "v")
                            }
                        )
                    })
            };
            yield return new object[]
                {
                new SampleStructuredData(
                    @"[a1 k1=""v1""][a2 k2=""v2""]",
                    new[]
                    {
                        new StructuredDataElement("a1",
                            new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("k1", "v1")
                            }
                        ),
                        new StructuredDataElement("a2",
                            new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("k2", "v2")
                            }
                        )
                    })
            };
            yield return new object[]
            {
                new SampleStructuredData(
                    @"[a1 k1=""v1"" k2=""v2""][a2 k3=""v3""]",
                    new[]
                    {
                        new StructuredDataElement("a1",
                            new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("k1", "v1"),
                                new KeyValuePair<string, string>("k2", "v2")
                            }
                        ),
                        new StructuredDataElement("a2",
                            new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("k3", "v3")
                            }
                        )
                    })
            };
            yield return new object[]
            {
                new SampleStructuredData(
                    @"[a1 k1=""v1"" k2=""v2"" k3=""v3""][a2 k4=""v4"" k5=""v5""][a3 k6=""v6""]",
                    new[]
                    {
                        new StructuredDataElement("a1",
                            new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("k1", "v1"),
                                new KeyValuePair<string, string>("k2", "v2"),
                                new KeyValuePair<string, string>("k3", "v3")
                            }
                        ),
                        new StructuredDataElement("a2",
                            new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("k4", "v4"),
                                new KeyValuePair<string, string>("k5", "v5")
                            }
                        ),
                        new StructuredDataElement("a3",
                             new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("k6", "v6")
                            }
                        )
                    })
            };
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

        public class SampleMessage
        {
            public string RawMessage { get; set; }
            public SyslogMessage ExpectedMessage { get; set; }

            public byte[] GetMessageBytes()
            {
                return Encoding.UTF8.GetBytes(RawMessage);
            }
        }
    }
}