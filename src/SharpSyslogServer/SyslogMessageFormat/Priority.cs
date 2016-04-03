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
    public class Priority : IEquatable<Priority>
    {
        public Facility Facility { get; set; }
        public Severity Severity { get; set; }

        public bool Equals(Priority other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Facility == other.Facility && Severity == other.Severity;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Priority) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Facility*397) ^ (int) Severity;
            }
        }

        public static bool operator ==(Priority left, Priority right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Priority left, Priority right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"Facility: {Facility}, Severity: {Severity}";
        }
    }
}