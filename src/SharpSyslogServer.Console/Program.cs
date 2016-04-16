using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Console;
using SystemConsole = System.Console;

namespace SharpSyslogServer.Console
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var loggerProvider = new ConsoleLoggerProvider((s, level) => true, false);
            var source = new CancellationTokenSource();

            var logger = new ConsoleMessageHandler();

            var syslogServer = new UdpSyslogServer( // DI?
                new LoggerRawMessageHandler(
                    new ParserRawMessageHandler(
                        new RegexSyslogMessageParser(TimeSpan.FromSeconds(1)),
                        logger),
                    loggerProvider.CreateLogger(nameof(LoggerRawMessageHandler)))
                );

            var exitKeyTask = PressEscToExit(logger);

            try
            {
                var syslogServerTask = syslogServer.Start(source.Token);
                Task.WaitAny(syslogServerTask, exitKeyTask);

                source.Cancel();
                syslogServerTask.Wait(TimeSpan.FromSeconds(3)); // throws errors from syslogServer, if any
                return 0;
            }
            catch (AggregateException aggEx)
            {
                if (aggEx.InnerExceptions.Any(p => p is OperationCanceledException))
                    return 0;
                throw;
            }
        }

        private static Task PressEscToExit(ConsoleMessageHandler handler)
        {
            return Task.Run(() =>
            {
                SystemConsole.WriteLine(@"Press:
P to print processed message count
C to clear screen
R to reset counter
ESC to exit");

                ConsoleKey key;
                while ((key = SystemConsole.ReadKey(true).Key) != ConsoleKey.Escape)
                {
                    switch (key)
                    {
                        case ConsoleKey.C:
                            handler.Clear();
                            continue;
                        case ConsoleKey.R:
                            handler.Reset();
                            continue;
                        case ConsoleKey.P:
                            SystemConsole.WriteLine(handler.GetCount());
                            continue;
                    }
                }
            });
        }
    }
}
