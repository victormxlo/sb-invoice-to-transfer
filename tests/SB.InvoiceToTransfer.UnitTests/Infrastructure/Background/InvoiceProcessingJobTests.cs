using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SB.InvoiceToTransfer.Application.UseCases.RunInvoiceProcessingJob;
using SB.InvoiceToTransfer.Infrastructure.Background;

namespace SB.InvoiceToTransfer.UnitTests.Infrastructure.Background
{
    public sealed class InvoiceProcessingJobTests
    {
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        private readonly Mock<IServiceScope> _scopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<InvoiceProcessingJob>> _loggerMock;
        private readonly IOptions<InvoiceProcessingJobOptions> _options;

        public InvoiceProcessingJobTests()
        {
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _scopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<InvoiceProcessingJob>>();

            _options = Options.Create(new InvoiceProcessingJobOptions
            {
                Interval = TimeSpan.FromMilliseconds(50)
            });

            _serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IMediator)))
                .Returns(_mediatorMock.Object);

            _scopeMock
                .SetupGet(s => s.ServiceProvider)
                .Returns(_serviceProviderMock.Object);

            _scopeFactoryMock
                .Setup(sf => sf.CreateScope())
                .Returns(_scopeMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldTriggerUseCase()
        {
            var tcs = new TaskCompletionSource();

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<RunInvoiceProcessingJobCommand>(),
                    It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    tcs.TrySetResult();

                    return Task.FromResult(
                        RunInvoiceProcessingJobResult.Ok(processed: 1)
                    );
                });

            var service = new InvoiceProcessingJob(
                scopeFactory: _scopeFactoryMock.Object,
                logger: _loggerMock.Object,
                options: _options);

            using var cts = new CancellationTokenSource();

            await service.StartAsync(cts.Token);

            await tcs.Task;

            cts.Cancel();
            await service.StopAsync(CancellationToken.None);

            _mediatorMock.Verify(
                m => m.Send(
                    It.IsAny<RunInvoiceProcessingJobCommand>(),
                    It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldNotThrow_WhenMediatorThrows()
        {
            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<RunInvoiceProcessingJobCommand>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Mediator failed"));

            using var cts = new CancellationTokenSource();

            var service = new InvoiceProcessingJob(
                scopeFactory: _scopeFactoryMock.Object,
                logger: _loggerMock.Object,
                options: _options);

            Func<Task> act = async () =>
            {
                await service.StartAsync(cts.Token);
                await Task.Delay(100);
                await service.StopAsync(CancellationToken.None);
            };

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldRespectCancellationToken()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var service = new InvoiceProcessingJob(
                options: _options,
                logger: _loggerMock.Object,
                scopeFactory: _scopeFactoryMock.Object);

            Func<Task> act = async () =>
            {
                await service.StartAsync(cts.Token);
            };

            await act.Should()
                .ThrowAsync<TaskCanceledException>();

            _mediatorMock.Verify(
                m => m.Send(
                    It.IsAny<RunInvoiceProcessingJobCommand>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
