using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SB.InvoiceToTransfer.Application.UseCases.RunInvoiceProcessingJob;

namespace SB.InvoiceToTransfer.Infrastructure.Background
{
    public sealed class InvoiceProcessingJob : BackgroundService
    {
        // Warm-up
        private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(5);

        private readonly IMediator _mediator;
        private readonly ILogger<InvoiceProcessingJob> _logger;
        private readonly TimeSpan _interval;

        public InvoiceProcessingJob(
            IMediator mediator,
            IOptions<InvoiceProcessingJobOptions> options,
            ILogger<InvoiceProcessingJob> logger)
        {
            _mediator = mediator;
            _logger = logger;
            _interval = options.Value.Interval;

            if (_interval <= TimeSpan.Zero)
            {
                throw new ArgumentException(
                    "InvoiceProcessingJobOptions.Interval must be greater than zero");
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
                        new RunInvoiceProcessingJobCommand(),
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
