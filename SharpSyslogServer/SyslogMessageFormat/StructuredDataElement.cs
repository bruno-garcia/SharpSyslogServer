using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpSyslogServer.SyslogMessageFormat
{
    /// <summary>
    /// An immutable, IEquatable, RFC 5424 Syslog Message Structured Data Element
    /// </summary>
    public sealed class StructuredDataElement : IEquatable<StructuredDataElement>
    {
        /// <summary>
        /// The Structured Data Element Identifier
        /// </summary>
        /// <summary>https://tools.ietf.org/html/rfc5424#section-6.3.1</summary>
        public string StructuredDataElementId { get; }
        /// <summary>
        /// Structured Data Element Parameters 
        /// </summary>
        /// <remarks>
        /// Dictionary: The same SD-ID MUST NOT exist more than once in a message.
        /// Key: 1*32PRINTUSASCII; except '=', SP, ']', %d34(")
        /// Value: UTF-8-STRING ; characters '"', '\' and ; ']' MUST be escaped.
        /// </remarks>
        public IReadOnlyCollection<KeyValuePair<string, string>> Parameters { get; }

        public StructuredDataElement(string structuredDataElementId, IReadOnlyCollection<KeyValuePair<string, string>> parameters)
        {
            StructuredDataElementId = structuredDataElementId;
            Parameters = parameters;
        }

        public bool Equals(StructuredDataElement other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(StructuredDataElementId, other.StructuredDataElementId)
                && (Equals(Parameters, other.Parameters)
                    || (Parameters.SequenceEqual(other.Parameters)));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((StructuredDataElement)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((StructuredDataElementId?.GetHashCode() ?? 0) * 397) ^ (Parameters != null ? Parameters.GetHashCode() : 0);
            }
        }

        public static bool operator ==(StructuredDataElement left, StructuredDataElement right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(StructuredDataElement left, StructuredDataElement right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"StructuredDataElementId: {StructuredDataElementId}, Parameters: [{string.Join(", ", Parameters.Select(e => $"{e.Key}={e.Value}"))}]";
        }
    }
}