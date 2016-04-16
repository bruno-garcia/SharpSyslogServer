using System.Threading;
using System.Threading.Tasks;

namespace SharpSyslogServer
{
    public interface ISyslogServer
    {
        Task Start(CancellationToken token);
    }
}