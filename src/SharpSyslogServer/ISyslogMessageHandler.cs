namespace SharpSyslogServer
{
    public interface ISyslogMessageHandler
    {
        void Handle(ISyslogMessage message);
    }
}