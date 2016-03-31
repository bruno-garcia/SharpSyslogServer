using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SystemConsole = System.Console;

namespace SharpSyslogServer.Console
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var source = new CancellationTokenSource();
            var sysLog = new UdpSyslogServer(new ConsoleSyslogMessageHandler());

            var exitKeyTask = PressEscToExit();

            try
            {
                var sysLogTask = Task.Run(() => sysLog.Start(source.Token), source.Token);
                Task.WaitAny(sysLogTask, exitKeyTask);

                source.Cancel();
                sysLogTask.Wait(TimeSpan.FromSeconds(3)); // throw syslogServer errors if any

                return 0;
            }
            catch (AggregateException aggEx)
            {
                if (aggEx.InnerExceptions.Any(p => p is OperationCanceledException))
                    return 0;
                throw;
            }
        }

        private static Task PressEscToExit()
        {
            return Task.Run(() =>
            {
                SystemConsole.WriteLine("Press ESC to exit");
                while (SystemConsole.ReadKey(true).Key != ConsoleKey.Escape)
                {
                }
            });
        }
    }
}
