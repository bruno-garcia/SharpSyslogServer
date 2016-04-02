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
    public class Priority
    {
        public Facility Facility { get; set; }
        public Severity Severity { get; set; }
    }
}