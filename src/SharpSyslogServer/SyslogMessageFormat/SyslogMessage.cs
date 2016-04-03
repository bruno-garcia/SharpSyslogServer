using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpSyslogServer.SyslogMessageFormat
{
    /// <summary>
    /// A Syslog Message as per RFC 5424
    /// </summary>
    public class SyslogMessage : IEquatable<SyslogMessage>
    {
        public Header Header { get; set; }
        /// <summary>
        /// Structured Data - Elements
        /// </summary>
        /// <remarks>https://tools.ietf.org/html/rfc5424#section-6.3</remarks>
        public IReadOnlyCollection<StructuredDataElement> StructuredData { get; set; }
        /// <summary>
        /// The syslog Message text
        /// </summary>
        public string Message { get; set; }

        public bool Equals(SyslogMessage other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Header, other.Header) 
                && (Equals(StructuredData, other.StructuredData) 
                    || (StructuredData.Count == other.StructuredData.Count 
                        && !StructuredData.Except(other.StructuredData).Any()))
                && string.Equals(Message, other.Message);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SyslogMessage) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Header != null ? Header.GetHashCode() : 0;
                hashCode = (hashCode*397) ^ (StructuredData != null ? StructuredData.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Message != null ? Message.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(SyslogMessage left, SyslogMessage right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SyslogMessage left, SyslogMessage right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            string structuredDataString = null;
            if (StructuredData != null)
                structuredDataString = string.Join(", ", StructuredData.Select(e => e.ToString()));

            return $"Header: {Header}, StructuredData: [{structuredDataString}], Message: {Message}";
        }
    }
}
