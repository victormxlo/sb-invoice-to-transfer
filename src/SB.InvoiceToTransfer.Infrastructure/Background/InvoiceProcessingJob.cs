using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SB.InvoiceToTransfer.Application.UseCases.RunInvoiceProcessingJob;

namespace SB.InvoiceToTransfer.Infrastructure.Background
{
    public sealed class InvoiceProcessingJob : BackgroundService
    {
        // Warm-up
        private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(30);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<InvoiceProcessingJob> _logger;
        private readonly TimeSpan _interval;

        public InvoiceProcessingJob(
            IServiceScopeFactory scopeFactory,
            ILogger<InvoiceProcessingJob> logger,
            IOptions<InvoiceProcessingJobOptions> options)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _interval = options.Value.Interval;

            if (_interval <= TimeSpan.Zero)
            {
                throw new ArgumentException(
                    "InvoiceProcessingJobOptions.Interval must be greater than zero");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Invoice processing job started. Interval: {Interval}",
                _interval);

            await Task.Delay(InitialDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    _logger.LogInformation(
                        "Triggering invoice processing execution");

                    await mediator.Send(
                        new RunInvoiceProcessingJobCommand(),
                        stoppingToken);

                    _logger.LogInformation(
                        "Invoice processing execution finished");
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation(
                        "Invoice processing job cancellation requested");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while executing invoice processing");
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
                "Invoice processing job stopped");
        }
    }
}
