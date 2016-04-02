using System;

namespace SharpSyslogServer.SyslogMessageFormat
{
    /// <summary>
    /// The Version of the syslog protocol specification
    /// </summary>
    /// <remarks>
    /// https://tools.ietf.org/html/rfc5424#section-6.2.2
    /// </remarks>
    public struct Version
    {
        private readonly byte _version;

        public Version(byte version)
        {
            if (version == 0 || version > 99)
                throw new ArgumentOutOfRangeException(nameof(version), "Version must be a non-zero, two digit long.");
            _version = version;
        }

        public static implicit operator byte(Version version)
        {
            return version._version;
        }

        public static implicit operator Version(byte version)
        {
            return new Version(version);
        }

        public override string ToString()
        {
            return $"Version: {_version}";
        }
    }
}