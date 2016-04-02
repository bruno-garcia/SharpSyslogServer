using System.Text;

namespace SharpSyslogServer.Console
{
    public class ConsoleRawMessageHandler : IRawMessageHandler
    {
        public void Handle(IRawMessage rawMessage)
        {
            var message = Encoding.UTF8.GetString(rawMessage.Payload, 0, rawMessage.Payload.Length);
            System.Console.WriteLine($"{rawMessage.ReceivedAt.ToString("o")} From: {rawMessage.RemoteEndPoint} Payload: {message}");
        }
    }
}
