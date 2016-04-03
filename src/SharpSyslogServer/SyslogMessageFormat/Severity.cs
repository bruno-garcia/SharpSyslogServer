namespace SharpSyslogServer.SyslogMessageFormat
{
    /// <summary>
    /// Message Severity
    /// </summary>
    /// <remarks>
    /// Severity values MUST be in the range of 0 to 7 inclusive.
    /// https://tools.ietf.org/html/rfc5424#section-6.2.1
    /// </remarks>
    public enum Severity : byte
    {
        /// <summary>
        /// System is unusable   
        /// </summary>
        Emergency = 0,
        /// <summary>
        /// Action must be taken immediately
        /// </summary>
        Alert = 1,
        /// <summary>
        /// Critical conditions
        /// </summary>
        Critical = 2,
        /// <summary>
        /// Error conditions
        /// </summary>
        Error = 3,
        /// <summary>
        /// Warning conditions
        /// </summary>
        Warning = 4,
        /// <summary>
        /// Normal but significant condition
        /// </summary>
        Notice = 5,
        /// <summary>
        /// Informational messages
        /// </summary>
        Informational = 6,
        /// <summary>
        /// Debug-level messages
        /// </summary>
        Debug = 7
    }
}