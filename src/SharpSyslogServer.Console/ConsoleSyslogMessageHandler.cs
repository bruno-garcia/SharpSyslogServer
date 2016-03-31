namespace SharpSyslogServer.Console
{
    public class ConsoleSyslogMessageHandler : ISyslogMessageHandler
    {
        public void Handle(ISyslogMessage message)
        {
            System.Console.WriteLine($"{message.ReceivedAt.ToString("o")} From: {message.RemoteEndPoint} Data: {message.Message}");
        }
    }
}
