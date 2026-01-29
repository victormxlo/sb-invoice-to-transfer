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
                _logger.LogInformation(
                    "Processing invoice {InvoiceExternalId} credit webhook", request.InvoiceExternalId);

                var invoice = await _invoiceRepository
                    .GetByExternalIdAsync(request.InvoiceExternalId, cancellationToken);

                if (invoice is null)
                {
                    _logger.LogWarning("Invoice {InvoiceExternalId} not found", request.InvoiceExternalId);
                    return ProcessInvoiceCreditResult.NotProcessed("Invoice not found");
                }

                if (invoice.Status is InvoiceStatus.Paid)
                {
                    _logger.LogInformation(
                        "Invoice {InvoiceExternalId} already processed. Ignoring webhook.", invoice.ExternalId);
                    return ProcessInvoiceCreditResult.AlreadyProcessed();
                }

                if (request.Amount <= 0 || request.Fee < 0)
                {
                    _logger.LogError(
                        "Invalid values for the invoice {InvoiceExternalId}. Amount: {Amount}, Fee: {Fee}",
                        invoice.ExternalId,
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

                if (invoice.Status == InvoiceStatus.Created)
                {
                    invoice.AssignAmountPaid(request.Amount / 100m);
                    invoice.AssignFee(request.Fee / 100m);

                    invoice.MarkAsProcessing();
                    await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
                }

                _logger.LogInformation(
                    "Creating transfer for invoice {InvoiceExternalId}",
                    invoice.ExternalId);

                var transferResult = await _starkBankClient
                    .CreateTransferAsync(netAmount, cancellationToken);

                if (!transferResult.Success)
                {
                    _logger.LogError(
                        "Invoice {InvoiceExternalId} transfer failed. ErrorCode: {ErrorCode}, Message: {Message}",
                        invoice.ExternalId,
                        transferResult.ErrorCode,
                        transferResult.ErrorMessage);

                    return ProcessInvoiceCreditResult.NotProcessed("Transfer failed");
                }

                invoice.MarkAsPaid(
                    amountPaid: netAmount / 100m,
                    fee: request.Fee / 100m,
                    transferId: transferResult.Data!
                );

                await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

                _logger.LogInformation(
                    "Invoice {InvoiceExternalId} processed successfully. TransferId: {TransferId}",
                    invoice.ExternalId, transferResult.Data);

                return ProcessInvoiceCreditResult.Ok(transferResult.Data!);
            }
        }
    }
}
