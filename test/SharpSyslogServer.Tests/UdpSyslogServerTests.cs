using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpSyslogServer;
using SharpSyslogServer.Networking;
using Moq;
using Xunit;

namespace SharpSyslogServerTests
{
    public class UdpSyslogServerTests
    {
        private class Fixture
        {
            public Mock<ISyslogMessageHandler> SyslogMessageHandlerMock { get; set; } = new Mock<ISyslogMessageHandler>();
            public Mock<IUdpClient> UpdClient { get; } = new Mock<IUdpClient>();
            public Mock<Func<IUdpClient>> UpdClientFactoryMock { get; set; } = new Mock<Func<IUdpClient>>();
            public Mock<Func<DateTime>> DateTimeFuncMock { get; set; } = new Mock<Func<DateTime>>();

            public Fixture()
            {
                UpdClientFactoryMock.Setup(f => f()).Returns(UpdClient.Object);
                DateTimeFuncMock.Setup(d => d()).Returns(() => DateTime.UtcNow);
            }

            public UdpSyslogServer GetSut()
            {
                return new UdpSyslogServer(SyslogMessageHandlerMock?.Object, UpdClientFactoryMock?.Object, DateTimeFuncMock?.Object);
            }
        }

        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void Start_UdpReceivedResult_SyslogMessageHandlerCalled()
        {
            // Arrange
            const string expectedMessage = "an expected message";
            var expectedEndpoint = new IPEndPoint(IPAddress.Loopback, 0);
            var expectedDateTime = DateTime.UtcNow;

            _fixture.DateTimeFuncMock.Setup(d => d()).Returns(() => expectedDateTime);
            _fixture.UpdClient.Setup(u => u.ReceiveAsync()).ReturnsAsync(
                new UdpReceiveResult(Encoding.UTF8.GetBytes(expectedMessage), expectedEndpoint));

            var handleCalledEvent = new ManualResetEventSlim();
            _fixture.SyslogMessageHandlerMock
                .Setup(h => h.Handle(It.Is<ISyslogMessage>(m => m.Message == expectedMessage
                                && Equals(m.RemoteEndPoint, expectedEndpoint)
                                && m.ReceivedAt == expectedDateTime)))
                .Callback<ISyslogMessage>(_ => handleCalledEvent.Set());

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
            _fixture.UpdClient.Setup(u => u.ReceiveAsync()).ReturnsAsync(new UdpReceiveResult());
            var ex = new Exception("Some error");
            var handleCalledEvent = new ManualResetEventSlim();
            _fixture.SyslogMessageHandlerMock
                .Setup(h => h.Handle(It.IsAny<ISyslogMessage>()))
                .Callback(handleCalledEvent.Set)
                .Throws(ex);

            var target = _fixture.GetSut();
            var source = new CancellationTokenSource();
            var server = target.Start(source.Token);

            Assert.True(handleCalledEvent.Wait(TimeSpan.FromSeconds(1)), "Handle never called");
            Assert.True(server.IsFaulted);
            source.Cancel();
        }

        [Fact]
        public async Task Start_CancelledToken_DoesNotStartReceivingData()
        {
            var source = new CancellationTokenSource();
            _fixture.UpdClientFactoryMock.Setup(f => f()).Callback(source.Cancel);

            var target = _fixture.GetSut();
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await target.Start(source.Token);
            });

            _fixture.UpdClientFactoryMock.Verify(f => f(), Times.Once);
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

        [Fact]
        public void Constructor_NullNowFunc_ThrowsArgumentNullException()
        {
            _fixture.DateTimeFuncMock = null;
            Assert.Throws<ArgumentNullException>(() => _fixture.GetSut());
        }
    }
}
