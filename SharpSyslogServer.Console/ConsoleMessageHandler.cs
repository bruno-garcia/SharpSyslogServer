using System.Threading;
using SharpSyslogServer.SyslogMessageFormat;
using SystemConsole = System.Console;

namespace SharpSyslogServer.Console
{
    public sealed class ConsoleMessageHandler : ISyslogMessageHandler
    {
        private long _counter;

        public long GetCount()
        {
            return Interlocked.Read(ref _counter);
        }

        public void Clear()
        {
            SystemConsole.Clear();
        }

        public void Reset()
        {
            Interlocked.Exchange(ref _counter, 0);
        }

        public void Handle(SyslogMessage syslogMessage)
        {
            Interlocked.Increment(ref _counter);
            SystemConsole.WriteLine(syslogMessage);
        }
    }
}
