using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SB.InvoiceToTransfer.Application.UseCases.CreateInvoices;
using SB.InvoiceToTransfer.Infrastructure.Background;

namespace SB.InvoiceToTransfer.UnitTests.Infrastructure.Background
{
    public sealed class InvoiceIssuanceSchedulerServiceTests
    {
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        private readonly Mock<IServiceScope> _scopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<InvoiceIssuanceSchedulerService>> _loggerMock;

        public InvoiceIssuanceSchedulerServiceTests()
        {
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _scopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<InvoiceIssuanceSchedulerService>>();

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

        private static IOptions<InvoiceIssuanceSchedulerOptions> CreateOptions(
            TimeSpan interval)
        {
            return Options.Create(new InvoiceIssuanceSchedulerOptions
            {
                Interval = interval
            });
        }

        [Fact]
        public async Task ExecuteAsync_ShouldTriggerInvoiceIssuance()
        {
            var tcs = new TaskCompletionSource();

            var result = new CreateInvoicesResult(
                Quantity: 1,
                ExternalInvoiceIds: new[] { "inv_123" },
                ExecutedAt: DateTime.UtcNow
            );

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<CreateInvoicesCommand>(),
                    It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    tcs.TrySetResult();
                    return Task.FromResult(result);
                });

            var service = new InvoiceIssuanceSchedulerService(
                scopeFactory: _scopeFactoryMock.Object,
                logger: _loggerMock.Object,
                options: CreateOptions(TimeSpan.FromMilliseconds(50)));

            using var cts = new CancellationTokenSource();

            await service.StartAsync(cts.Token);

            await tcs.Task;

            cts.Cancel();
            await service.StopAsync(CancellationToken.None);

            _mediatorMock.Verify(
                m => m.Send(
                    It.IsAny<CreateInvoicesCommand>(),
                    It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldNotThrow_WhenMediatorThrowsException()
        {
            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<CreateInvoicesCommand>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            var service = new InvoiceIssuanceSchedulerService(
                scopeFactory: _scopeFactoryMock.Object,
                options: CreateOptions(TimeSpan.FromMilliseconds(50)),
                logger: _loggerMock.Object);

            using var cts = new CancellationTokenSource(200);

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

            var service = new InvoiceIssuanceSchedulerService(
                scopeFactory: _scopeFactoryMock.Object,
                options: CreateOptions(TimeSpan.FromMilliseconds(50)),
                logger: _loggerMock.Object);

            Func<Task> act = async () =>
            {
                await service.StartAsync(cts.Token);
            };

            await act.Should()
                .ThrowAsync<TaskCanceledException>();

            _mediatorMock.Verify(
                m => m.Send(
                    It.IsAny<CreateInvoicesCommand>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenIntervalIsInvalid()
        {
            var options = CreateOptions(TimeSpan.Zero);

            Action act = () => new InvoiceIssuanceSchedulerService(
                scopeFactory: _scopeFactoryMock.Object,
                logger: _loggerMock.Object,
                options: options);

            act.Should()
               .Throw<ArgumentException>()
               .WithMessage("*Interval must be greater than zero*");
        }
    }
}
