using System.Net.Sockets;
using System.Threading.Tasks;

namespace SharpSyslogServer.Networking
{
    internal sealed class UdpClientAdapter : IUdpClient
    {
        private readonly UdpClient _udpClient;

        public UdpClientAdapter(int port)
        {
            _udpClient = new UdpClient(port);
        }

        public async Task<UdpReceiveResult> ReceiveAsync()
        {
            return await _udpClient.ReceiveAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            _udpClient.Dispose();
        }
    }
}
