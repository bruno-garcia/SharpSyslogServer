using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SharpSyslogServer
{
    internal static class MatchExtensions
    {
        public static string GroupSuccessValue(this Match match, string groupName)
        {
            var group = match.Groups[groupName];
            return @group.Success ? @group.Value : null;
        }

        /// <summary>
        /// If <paramref name="groupName"/> evaluates as Success, tries to parse the Value as Byte
        /// </summary>
        /// <param name="match">The Match containing the Group with name <paramref name="groupName"/></param>
        /// <param name="groupName">Name of the group to parse the value as Byte</param>
        /// <returns>Byte value, or null if not success/failed to parse</returns>
        public static byte? ParseSuccessfulByte(this Match match, string groupName)
        {
            var group = match.Groups[groupName];

            byte @byte;
            if (group.Success && byte.TryParse(group.Value, out @byte))
                return @byte;

            return null;
        }

        /// <summary>
        /// If <paramref name="groupName"/> evaluates as Success, tries to parse the Value as Double
        /// </summary>
        /// <param name="match">The Match containing the Group with name <paramref name="groupName"/></param>
        /// <param name="groupName">Name of the group to parse the value as Double</param>
        /// <returns>Double value, or null if not success/failed to parse</returns>
        public static double? ParseSuccessfulDouble(this Match match, string groupName)
        {
            var group = match.Groups[groupName];

            byte @byte;
            if (group.Success && byte.TryParse(group.Value, out @byte))
                return @byte;

            return null;
        }

        public static DateTimeOffset? ParseTimestamp(this Match match, string groupName)
        {
            var group = match.Groups[groupName];
            DateTimeOffset dateTimeOffset;
            if (group.Success && TryParseTimestamp(group.Value, out dateTimeOffset))
                return dateTimeOffset;
            return null;
        }

        internal static bool TryParseTimestamp(string timestamp, out DateTimeOffset eventTime)
        {
            return DateTimeOffset.TryParse(
                timestamp,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out eventTime);
        }
    }
}
