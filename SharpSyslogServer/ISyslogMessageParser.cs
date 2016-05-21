using SharpSyslogServer.SyslogMessageFormat;

namespace SharpSyslogServer
{
    public interface ISyslogMessageParser
    {
        SyslogMessage Parse(byte[] payload);
        bool TryParse(byte[] payload, out SyslogMessage syslogMessage);
    }
}