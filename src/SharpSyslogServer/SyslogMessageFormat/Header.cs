using System;

namespace SharpSyslogServer.SyslogMessageFormat
{
    /// <summary>
    /// Syslog Message Header
    /// </summary>
    /// <remarks>https://tools.ietf.org/html/rfc5424#section-6</remarks>
    public class Header
    {
        /// <summary>
        /// Priority: Facility and Severity
        /// </summary>
        public Priority Priority { get; set; }
        /// <summary>
        /// Protocol Version
        /// </summary>
        public Version Version { get; set; }
        /// <summary>
        /// Time of the event
        /// </summary>
        /// <remarks>NILVALUE / FULL-DATE "T" FULL-TIME</remarks>
        public DateTime EventTime { get; set; }
        /// <summary>
        /// Hostname
        /// </summary>
        /// <remarks>NILVALUE / 1*255PRINTUSASCII</remarks>
        public string Hostname { get; set; }
        /// <summary>
        /// Application Name
        /// </summary>
        /// <remarks>NILVALUE / 1*48PRINTUSASCII</remarks>
        public string AppName { get; set; }
        /// <summary>
        /// Process Identifier
        /// </summary>
        /// <remarks>NILVALUE / 1*128PRINTUSASCII</remarks>
        public string ProcessId { get; set; }
        /// <summary>
        /// Message Identifier
        /// </summary>
        /// <remarks>NILVALUE / 1*32PRINTUSASCII</remarks>
        public string MessageId { get; set; }
    }
}