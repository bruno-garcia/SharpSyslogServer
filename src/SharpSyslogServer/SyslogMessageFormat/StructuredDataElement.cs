using System.Collections.Generic;

namespace SharpSyslogServer.SyslogMessageFormat
{
    public class StructuredDataElement
    {
        /// <summary>
        /// The Structured Data Element Identifier
        /// </summary>
        /// <summary>https://tools.ietf.org/html/rfc5424#section-6.3.1</summary>
        public string StructuredDataElementId { get; set; }
        /// <summary>
        /// Structured Data Element Parameters 
        /// </summary>
        /// <remarks>
        /// Dictionary: The same SD-ID MUST NOT exist more than once in a message.
        /// Key: 1*32PRINTUSASCII; except '=', SP, ']', %d34(")
        /// Value: UTF-8-STRING ; characters '"', '\' and ; ']' MUST be escaped.
        /// </remarks>
        public Dictionary<string, string> Parameters { get; set; }
    }
}