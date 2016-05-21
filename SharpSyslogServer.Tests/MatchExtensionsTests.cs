using System;
using System.Collections.Generic;
using SharpSyslogServer;
using Xunit;

namespace SharpSyslogServerTests
{
    public sealed class MatchExtensionsTests
    {
        [Theory]
        [MemberData(nameof(GetSampleValidDateTime))]
        public void Parse_ValidTimestamp_ParsesDateTimeOffset(SampleTimestamp testCase)
        {
            DateTimeOffset actual;
            Assert.True(MatchExtensions.TryParseTimestamp(testCase.RawTimestamp, out actual));
            Assert.Equal(testCase.ExpectedDateTimeOffset, actual);
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

        public sealed class SampleTimestamp
        {
            public string RawTimestamp { get; }
            public DateTimeOffset ExpectedDateTimeOffset { get; }

            public SampleTimestamp(string rawTimestamp, DateTimeOffset expectedDateTimeOffset)
            {
                RawTimestamp = rawTimestamp;
                ExpectedDateTimeOffset = expectedDateTimeOffset;
            }
        }
    }
}
