namespace SharpSyslogServer.SyslogMessageFormat
{
    /// <summary>
    /// A Syslog Message as per RFC 5424
    /// </summary>
    public class SyslogMessage
    {
        public Header Header { get; set; }
        /// <summary>
        /// Structured Data/Element
        /// </summary>
        public StructuredData StructuredData { get; set; }
        /// <summary>
        /// The syslog Message text
        /// </summary>
        public string Message { get; set; }
    }
}
