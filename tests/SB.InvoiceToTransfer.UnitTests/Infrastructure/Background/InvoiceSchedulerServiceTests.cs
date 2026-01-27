using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SB.InvoiceToTransfer.Application.UseCases.RunInvoiceScheduler;
using SB.InvoiceToTransfer.Infrastructure.Background;

namespace SB.InvoiceToTransfer.UnitTests.Infrastructure.Background
{
    public class InvoiceSchedulerServiceTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<InvoiceSchedulerService>> _loggerMock;
        private readonly IOptions<InvoiceSchedulerOptions> _options;

        public InvoiceSchedulerServiceTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<InvoiceSchedulerService>>();
            _options = Options.Create(new InvoiceSchedulerOptions
            {
                Interval = TimeSpan.FromMilliseconds(50)
            });
        }

        [Fact]
        public async Task ExecuteAsync_ShouldTriggerUseCase()
        {
            var tcs = new TaskCompletionSource();
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<RunInvoiceSchedulerCommand>(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    tcs.TrySetResult();
                    return Task.FromResult(Unit.Value);
                });

            var service = new InvoiceSchedulerService(_mediatorMock.Object, _options, _loggerMock.Object);

            using var cts = new CancellationTokenSource();

            var serviceTask = service.StartAsync(cts.Token);

            await tcs.Task;

            cts.Cancel();
            await service.StopAsync(CancellationToken.None);

            _mediatorMock.Verify(
                m => m.Send(It.IsAny<RunInvoiceSchedulerCommand>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public async Task ExecuteAsync_ShouldNotThrow_WhenMediatorThrows()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<RunInvoiceSchedulerCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Mediator failed"));

            using var cts = new CancellationTokenSource(200);
            var service = new InvoiceSchedulerService(_mediatorMock.Object, _options, _loggerMock.Object);

            Func<Task> act = async () =>
            {
                await service.StartAsync(cts.Token);
                await Task.Delay(100);
                await service.StopAsync(cts.Token);
            };

            await act.Should().NotThrowAsync<Exception>();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldRespectCancellationToken()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            var service = new InvoiceSchedulerService(_mediatorMock.Object, _options, _loggerMock.Object);

            Func<Task> act = async () =>
            {
                try
                {
                    await service.StartAsync(cts.Token);
                }
                catch (TaskCanceledException)
                {
                    // Expected behavior
                }
            };

            await act.Should().NotThrowAsync<Exception>();
            _mediatorMock.Verify(m => m.Send(
                It.IsAny<RunInvoiceSchedulerCommand>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
