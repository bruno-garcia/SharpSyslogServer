namespace SharpSyslogServer
{
    public interface IRawMessageHandler
    {
        void Handle(IRawMessage rawMessage);
    }
}