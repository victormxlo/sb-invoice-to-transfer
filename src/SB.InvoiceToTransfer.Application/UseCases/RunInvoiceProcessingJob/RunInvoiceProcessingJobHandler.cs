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
            _logger.LogInformation("Starting invoice processing job execution");

            var activeState = await _stateRepository.GetActiveAsync(cancellationToken);
            if (activeState is not null)
            {
                _logger.LogWarning(
                    "Invoice processing job is already running. StartedAt: {StartedAt}",
                    activeState.StartedAt);

                return RunInvoiceProcessingJobResult.Skipped("Processing job already running");
            }

            var processingJobState = new InvoiceProcessingJobState();
            await _stateRepository.AddAsync(processingJobState, cancellationToken);

            try
            {
                var invoices = await _invoiceRepository
                    .GetByStatusAsync(InvoiceStatus.Processing, cancellationToken);

                _logger.LogInformation(
                    "Found {Count} invoices eligible for processing",
                    invoices.Count);

                var processedCount = 0;

                foreach (var invoice in invoices)
                {
                    if (invoice.TransferId is not null)
                    {
                        _logger.LogInformation(
                            "Invoice {ExternalId} already has transfer. Skipping.",
                            invoice.ExternalId);

                        continue;
                    }

                    if (invoice.AmountPaid is null)
                    {
                        _logger.LogWarning(
                            "Invoice {ExternalId} is in processing state but has no AmountPaid. Skipping.",
                            invoice.ExternalId);

                        continue;
                    }

                    await _mediator.Send(
                        new ProcessInvoiceCredit.ProcessInvoiceCreditCommand
                        {
                            InvoiceExternalId = invoice.ExternalId!,
                            Amount = (long)Math.Round(invoice.AmountPaid.Value * 100, MidpointRounding.AwayFromZero),
                            Fee = invoice.Fee.HasValue
                            ? (long)Math.Round(invoice.Fee.Value * 100, MidpointRounding.AwayFromZero)
                            : 0
                        },
                        cancellationToken);

                    processedCount++;
                }

                _logger.LogInformation(
                    "Invoice processing job execution finished successfully. Processed: {Count}",
                    processedCount);

                return RunInvoiceProcessingJobResult.Ok(processedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while executing invoice processing job");
                throw;
            }
            finally
            {
                processingJobState.Finish();
                await _stateRepository.UpdateAsync(processingJobState, cancellationToken);

                _logger.LogInformation("Invoice processing job state finalized");
            }
        }
    }
}
