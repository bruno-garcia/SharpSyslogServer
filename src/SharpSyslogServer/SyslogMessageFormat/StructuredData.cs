using System.Collections.Generic;

namespace SharpSyslogServer.SyslogMessageFormat
{
    /// <summary>
    /// Represents the field Structured Data 
    /// </summary>
    /// <remarks>https://tools.ietf.org/html/rfc5424#section-6.3</remarks>
    public class StructuredData
    {
        /// <summary>
        /// Structured Data Identifier
        /// </summary>
        /// <remarks>SD-IDs are case-sensitive and uniquely identify the type and purpose
        /// of the SD-ELEMENT.
        /// https://tools.ietf.org/html/rfc5424#section-6.3.2
        /// </remarks>
        public string StructuredDataId { get; set; }
        /// <summary>
        /// Enumerable of Structured Data Elements
        /// </summary>
        public IEnumerable<StructuredDataElement> StructuredDataElements { get; set; }
    }
}