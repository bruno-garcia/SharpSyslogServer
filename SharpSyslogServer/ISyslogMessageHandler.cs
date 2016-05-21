using SharpSyslogServer.SyslogMessageFormat;

namespace SharpSyslogServer
{
    public interface ISyslogMessageHandler
    {
        void Handle(SyslogMessage syslogMessage);
    }
}
