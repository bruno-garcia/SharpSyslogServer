using System;

namespace SharpSyslogServer.SyslogMessageFormat
{
    /// <summary>
    /// Syslog Message Header
    /// </summary>
    /// <remarks>
    /// An immutable, IEquatable Syslog Message Header
    /// https://tools.ietf.org/html/rfc5424#section-6
    /// </remarks>
    public sealed class Header : IEquatable<Header>
    {
        /// <summary>
        /// Priority: Facility and Severity
        /// </summary>
        public Priority Priority { get; }
        /// <summary>
        /// Protocol Version
        /// </summary>
        public Version? Version { get; }
        /// <summary>
        /// Time of the event
        /// </summary>
        /// <remarks>NILVALUE / FULL-DATE "T" FULL-TIME</remarks>
        public DateTimeOffset? EventTime { get; }
        /// <summary>
        /// Hostname
        /// </summary>
        /// <remarks>NILVALUE / 1*255PRINTUSASCII</remarks>
        public string Hostname { get; }
        /// <summary>
        /// Application Name
        /// </summary>
        /// <remarks>NILVALUE / 1*48PRINTUSASCII</remarks>
        public string AppName { get; }
        /// <summary>
        /// Process Identifier
        /// </summary>
        /// <remarks>NILVALUE / 1*128PRINTUSASCII</remarks>
        public string ProcessId { get; }
        /// <summary>
        /// Message Identifier
        /// </summary>
        /// <remarks>NILVALUE / 1*32PRINTUSASCII</remarks>
        public string MessageId { get; }

        public Header(
            Priority priority,
            Version? version = null,
            DateTimeOffset? eventTime = null,
            string hostname = null,
            string appName = null,
            string processId = null,
            string messageId = null)
        {
            Priority = priority;
            Version = version;
            EventTime = eventTime;
            Hostname = hostname;
            AppName = appName;
            ProcessId = processId;
            MessageId = messageId;
        }

        internal Header() : this(new Priority()) { }

        public bool Equals(Header other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Priority, other.Priority) && Version.Equals(other.Version) && EventTime.Equals(other.EventTime) && string.Equals(Hostname, other.Hostname) && string.Equals(AppName, other.AppName) && string.Equals(ProcessId, other.ProcessId) && string.Equals(MessageId, other.MessageId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Header)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Priority != null ? Priority.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ Version.GetHashCode();
                hashCode = (hashCode * 397) ^ EventTime.GetHashCode();
                hashCode = (hashCode * 397) ^ (Hostname != null ? Hostname.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AppName != null ? AppName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ProcessId != null ? ProcessId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MessageId != null ? MessageId.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Header left, Header right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Header left, Header right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"Priority: {Priority}, Version: {Version}, EventTime: {EventTime}, Hostname: {Hostname}, AppName: {AppName}, ProcessId: {ProcessId}, MessageId: {MessageId}";
        }
    }
}