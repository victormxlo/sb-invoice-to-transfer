using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SB.InvoiceToTransfer.Application.UseCases.CreateInvoices;

namespace SB.InvoiceToTransfer.Infrastructure.Background
{
    public sealed class InvoiceIssuanceSchedulerService : BackgroundService
    {
        // Warm-up
        private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(20);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<InvoiceIssuanceSchedulerService> _logger;
        private readonly TimeSpan _interval;

        public InvoiceIssuanceSchedulerService(
            IServiceScopeFactory scopeFactory,
            ILogger<InvoiceIssuanceSchedulerService> logger,
            IOptions<InvoiceIssuanceSchedulerOptions> options)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _interval = options.Value.Interval;

            if (_interval <= TimeSpan.Zero)
            {
                throw new ArgumentException(
                    "InvoiceIssuanceSchedulerOptions.Interval must be greater than zero");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Invoice Issuance Scheduler started. Interval: {Interval}",
                _interval);

            await Task.Delay(InitialDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    _logger.LogInformation(
                        "Triggering invoice issuance execution");

                    await mediator.Send(
                        new CreateInvoicesCommand(),
                        stoppingToken);

                    _logger.LogInformation(
                        "Invoice issuance completed successfully");
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation(
                        "Invoice issuance Scheduler cancellation requested");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while issuing invoices");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation(
                "Invoice issuance Scheduler stopped");
        }
    }
}
