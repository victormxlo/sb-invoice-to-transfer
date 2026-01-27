using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SB.InvoiceToTransfer.Application.UseCases.RunInvoiceScheduler;

namespace SB.InvoiceToTransfer.Infrastructure.Background
{
    public sealed class InvoiceSchedulerService : BackgroundService
    {
        // Warm-up
        private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(5);

        private readonly IMediator _mediator;
        private readonly ILogger<InvoiceSchedulerService> _logger;
        private readonly TimeSpan _interval;

        public InvoiceSchedulerService(
            IMediator mediator,
            IOptions<InvoiceSchedulerOptions> options,
            ILogger<InvoiceSchedulerService> logger)
        {
            _mediator = mediator;
            _logger = logger;
            _interval = options.Value.Interval;

            if (_interval <= TimeSpan.Zero)
            {
                throw new ArgumentException(
                    "InvoiceSchedulerOptions.Interval must be greater than zero");
            }
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Invoice Scheduler Service started. Interval: {Interval}",
                _interval);

            await Task.Delay(InitialDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation(
                        "Triggering invoice scheduler execution");

                    await _mediator.Send(
                        new RunInvoiceSchedulerCommand(),
                        stoppingToken);

                    _logger.LogInformation(
                        "Invoice scheduler execution finished");
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation(
                        "Invoice Scheduler Service cancellation requested");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while executing invoice scheduler");
                }

                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation(
                "Invoice Scheduler Service stopped");
        }
    }
}
