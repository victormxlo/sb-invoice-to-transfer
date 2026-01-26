using MediatR;
using Microsoft.Extensions.Logging;
using SB.InvoiceToTransfer.Application.Interfaces.External;
using SB.InvoiceToTransfer.Application.Interfaces.Repositories;
using SB.InvoiceToTransfer.Domain.Enums;

namespace SB.InvoiceToTransfer.Application.UseCases.ProcessInvoiceCredit
{
    public sealed class ProcessInvoiceCreditHandler : IRequestHandler<ProcessInvoiceCreditCommand, ProcessInvoiceCreditResult>
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IStarkBankClient _starkBankClient;
        private ILogger<ProcessInvoiceCreditHandler> _logger;

        public ProcessInvoiceCreditHandler(
            IInvoiceRepository invoiceRepository,
            IStarkBankClient starkBankClient,
            ILogger<ProcessInvoiceCreditHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _starkBankClient = starkBankClient;
            _logger = logger;
        }

        public async Task<ProcessInvoiceCreditResult> Handle(
            ProcessInvoiceCreditCommand request,
            CancellationToken cancellationToken)
        {
            using (_logger.BeginScope(
                "InvoiceExternalId: {ExternalId}",
                request.InvoiceExternalId))
            {
                _logger.LogInformation("Processing invoice credit webhook");

                var invoice = await _invoiceRepository
                    .GetByExternalIdAsync(request.InvoiceExternalId, cancellationToken);

                if (invoice is null)
                {
                    _logger.LogWarning("Invoice not found");
                    return ProcessInvoiceCreditResult.NotProcessed("Invoice not found");
                }

                if (invoice.Status is InvoiceStatus.Paid)
                {
                    _logger.LogInformation("Invoice already processed. Ignoring webhook.");
                    return ProcessInvoiceCreditResult.AlreadyProcessed();
                }

                if (request.Amount <= 0 || request.Fee < 0)
                {
                    _logger.LogError(
                        "Invalid values. Amount: {Amount}, Fee: {Fee}",
                        request.Amount,
                        request.Fee);

                    return ProcessInvoiceCreditResult.NotProcessed("Invalid financial values");
                }

                var netAmount = request.Amount - request.Fee;

                if (netAmount <= 0)
                {
                    _logger.LogError(
                        "Net amount must be greater than zero. NetAmount: {NetAmount}",
                        netAmount);

                    return ProcessInvoiceCreditResult.NotProcessed("Invalid net amount");
                }

                if (invoice.Status != InvoiceStatus.Created)
                {
                    _logger.LogInformation("Invoice already being processed");
                    return ProcessInvoiceCreditResult.AlreadyProcessed();
                }

                invoice.MarkAsProcessing();
                await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

                var transferResult = await _starkBankClient
                    .CreateTransferAsync(netAmount, cancellationToken);

                if (!transferResult.Success)
                {
                    _logger.LogError(
                        "Transfer failed. ErrorCode: {ErrorCode}, Message: {Message}",
                        transferResult.ErrorCode,
                        transferResult.ErrorMessage);

                    return ProcessInvoiceCreditResult.NotProcessed("Transfer failed");
                }

                invoice.MarkAsPaid(netAmount, transferResult.Data!);

                await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

                _logger.LogInformation(
                    "Invoice processed successfully. TransferId: {TransferId}",
                    transferResult.Data);

                return ProcessInvoiceCreditResult.Ok(transferResult.Data!);
            }
        }
    }
}
