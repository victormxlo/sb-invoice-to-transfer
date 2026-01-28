using MediatR;
using Microsoft.Extensions.Logging;
using SB.InvoiceToTransfer.Application.Interfaces.Repositories;
using SB.InvoiceToTransfer.Application.Models;
using SB.InvoiceToTransfer.Domain.Enums;

namespace SB.InvoiceToTransfer.Application.UseCases.RunInvoiceProcessingJob
{
    public sealed class RunInvoiceProcessingJobHandler
    : IRequestHandler<RunInvoiceProcessingJobCommand, RunInvoiceProcessingJobResult>
    {
        private readonly IInvoiceProcessingJobStateRepository _stateRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<RunInvoiceProcessingJobHandler> _logger;

        public RunInvoiceProcessingJobHandler(
            IInvoiceProcessingJobStateRepository stateRepository,
            IInvoiceRepository invoiceRepository,
            IMediator mediator,
            ILogger<RunInvoiceProcessingJobHandler> logger)
        {
            _stateRepository = stateRepository;
            _invoiceRepository = invoiceRepository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<RunInvoiceProcessingJobResult> Handle(RunInvoiceProcessingJobCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Invoice Scheduler execution");

            var activeState = await _stateRepository.GetActiveAsync(cancellationToken);
            if (activeState is not null)
            {
                _logger.LogWarning(
                    "Invoice Scheduler is already running. StartedAt: {StartedAt}",
                    activeState.StartedAt);

                return RunInvoiceProcessingJobResult.Skipped("Scheduler already running");
            }

            var schedulerState = new InvoiceProcessingJobState();
            await _stateRepository.AddAsync(schedulerState, cancellationToken);

            try
            {
                var invoices = await _invoiceRepository
                    .GetByStatusAsync(InvoiceStatus.Created, cancellationToken);

                _logger.LogInformation(
                    "Found {Count} invoices eligible for processing",
                    invoices.Count);

                var processedCount = 0;

                foreach (var invoice in invoices)
                {
                    await _mediator.Send(
                        new ProcessInvoiceCredit.ProcessInvoiceCreditCommand
                        {
                            InvoiceExternalId = invoice.ExternalId!,
                            Amount = (long)Math.Round(invoice.Amount * 100, MidpointRounding.AwayFromZero),
                            Fee = invoice.Fee > 0 ? (long)Math.Round(invoice.Fee.Value * 100, MidpointRounding.AwayFromZero) : 0
                        },
                        cancellationToken);

                    processedCount++;
                }

                _logger.LogInformation(
                    "Invoice Scheduler execution finished successfully. Processed: {Count}",
                    processedCount);

                return RunInvoiceProcessingJobResult.Ok(processedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while executing Invoice Scheduler");
                throw;
            }
            finally
            {
                schedulerState.Finish();
                await _stateRepository.UpdateAsync(schedulerState, cancellationToken);

                _logger.LogInformation("Invoice Scheduler state finalized");
            }
        }
    }
}
