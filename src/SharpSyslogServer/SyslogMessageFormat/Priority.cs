using System;

namespace SharpSyslogServer.SyslogMessageFormat
{
    /// <summary>
    /// Message Priority, Composed by Facility and Severity
    /// </summary>
    /// <remarks>
    /// Facility and Severity values are not normative but often used.  They
    /// are described in the following tables for purely informational
    /// purposes. 
    /// https://tools.ietf.org/html/rfc5424#section-6.2.1
    /// </remarks>
    public struct Priority : IEquatable<Priority>
    {
        public Facility Facility { get; }
        public Severity Severity { get; }

        internal Priority(byte facility, byte severity)
            : this((Facility)facility, (Severity)severity)
        {
        }

        public Priority(Facility facility, Severity severity)
        {
            Facility = facility;
            Severity = severity;
        }

        public bool Equals(Priority other)
        {
            return Facility == other.Facility && Severity == other.Severity;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Priority && Equals((Priority)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Facility * 397) ^ (int)Severity;
            }
        }

        public static bool operator ==(Priority left, Priority right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Priority left, Priority right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"Facility: {Facility}, Severity: {Severity}";
        }
    }
}