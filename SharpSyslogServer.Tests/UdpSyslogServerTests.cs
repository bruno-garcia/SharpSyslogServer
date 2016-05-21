using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpSyslogServer;
using SharpSyslogServer.Networking;
using Moq;
using SharpSyslogServer.Transport;
using Xunit;

namespace SharpSyslogServerTests
{
    public sealed class UdpSyslogServerTests
    {
        private sealed class Fixture
        {
            public Mock<IRawMessageHandler> SyslogMessageHandlerMock { get; set; } = new Mock<IRawMessageHandler>();
            public Mock<IUdpClient> UpdClient { get; } = new Mock<IUdpClient>();
            public Mock<Func<IUdpClient>> UpdClientFactoryMock { get; set; } = new Mock<Func<IUdpClient>>();
            public UdpReceiveResult UdpReceiveResult { get; }
            public byte[] Payload { get; } = Encoding.UTF8.GetBytes("A message");

            public Fixture()
            {
                UdpReceiveResult = new UdpReceiveResult(Payload, new IPEndPoint(IPAddress.Loopback, 0));

                UpdClientFactoryMock.Setup(f => f()).Returns(UpdClient.Object);
                UpdClient.Setup(u => u.ReceiveAsync()).ReturnsAsync(UdpReceiveResult);
            }

            public UdpSyslogServer GetSut()
            {
                return new UdpSyslogServer(SyslogMessageHandlerMock?.Object, UpdClientFactoryMock?.Object);
            }
        }

        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void Start_UdpReceivedResult_SyslogMessageHandlerCalled()
        {
            // Arrange
            var expectedDateTime = DateTime.UtcNow;

            var handleCalledEvent = new ManualResetEventSlim();
            _fixture.SyslogMessageHandlerMock
                .Setup(h => h.Handle(It.Is<IRawMessage>(m => m.Payload == _fixture.Payload
                                && Equals(m.RemoteEndPoint, _fixture.UdpReceiveResult.RemoteEndPoint))))
                .Callback<IRawMessage>(_ => handleCalledEvent.Set());

            // Act
            var target = _fixture.GetSut();
            var source = new CancellationTokenSource();
            target.Start(source.Token);

            // Assert
            Assert.True(handleCalledEvent.Wait(TimeSpan.FromSeconds(1)), "Handle never called with correct arguments");
            source.Cancel();
        }

        [Fact]
        public void Start_SyslogMessageHandlerThrows_TaskFinishesAsFaulted()
        {
            var expected = new Exception("Some error");
            _fixture.SyslogMessageHandlerMock
                .Setup(h => h.Handle(It.IsAny<IRawMessage>()))
                .Throws(expected);

            var target = _fixture.GetSut();
            var server = target.Start(CancellationToken.None);

            var actual = Assert.Throws<AggregateException>(() => server.Wait(TimeSpan.FromSeconds(1)));

            Assert.Same(expected, actual.InnerExceptions.Single());
        }

        [Fact]
        public async Task Start_CancelledToken_DoesNotStartReceivingData()
        {
            var source = new CancellationTokenSource();
            source.Cancel();
            _fixture.UpdClientFactoryMock.Setup(f => f()).Returns(_fixture.UpdClient.Object);

            var target = _fixture.GetSut();
            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await target.Start(source.Token);
            });

            _fixture.UpdClientFactoryMock.Verify(f => f(), Times.Never);
            _fixture.UpdClient.Verify(u => u.ReceiveAsync(), Times.Never);
        }

        [Fact]
        public async Task Start_CancelledToken_DoesNotCallFactory()
        {
            var source = new CancellationTokenSource();
            source.Cancel();

            var target = _fixture.GetSut();
            await Assert.ThrowsAsync<TaskCanceledException>(() => target.Start(source.Token));

            _fixture.UpdClientFactoryMock.Verify(f => f(), Times.Never);
        }

        [Fact]
        public void Constructor_NullSyslogMessageHandler_ThrowsArgumentNullException()
        {
            _fixture.SyslogMessageHandlerMock = null;
            Assert.Throws<ArgumentNullException>(() => _fixture.GetSut());
        }

        [Fact]
        public void Constructor_NullUpdClientFactory_ThrowsArgumentNullException()
        {
            _fixture.UpdClientFactoryMock = null;
            Assert.Throws<ArgumentNullException>(() => _fixture.GetSut());
        }
    }
}
