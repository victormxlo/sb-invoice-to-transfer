using Microsoft.Extensions.Logging;
using SB.InvoiceToTransfer.Application.Abstractions.External;
using SB.InvoiceToTransfer.Application.Interfaces.External;
using SB.InvoiceToTransfer.Infrastructure.Configuration;
using StarkBank;
using StarkBank.Error;

namespace SB.InvoiceToTransfer.Infrastructure.External.Banking
{
    public sealed class StarkBankClient : IStarkBankClient
    {
        private readonly ILogger<StarkBankClient> _logger;

        public StarkBankClient(ILogger<StarkBankClient> logger)
        {
            _logger = logger;

            var project = new Project(
               environment: Secrets.Require("SB_ENVIRONMENT"),
               id: Secrets.Require("SB_PROJECT_ID"),
               privateKey: Secrets.Require("SB_PRIVATE_KEY")
            );

            StarkBank.Settings.User = project;
        }

        public async Task<StarkBankOperationResult<IEnumerable<string>>> CreateInvoicesAsync(
            IReadOnlyCollection<Domain.Invoice> invoices, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Starting creation of {Count} invoices in Stark Bank", invoices.Count);

            try
            {
                if (invoices == null || invoices.Count == 0)
                    return StarkBankOperationResult<IEnumerable<string>>
                        .Ok(Array.Empty<string>());

                _logger.LogInformation(
                    "Creating {Count} invoices in Stark Bank", invoices.Count);

                var starkInvoices = invoices.Select(i => new StarkBank.Invoice(
                    amount: (long)Math.Round(i.Amount * 100, MidpointRounding.AwayFromZero),
                    name: i.RecipientName,
                    taxID: i.TaxId,
                    due: i.DueDate)).ToList();

                var created = await Task.Run(
                    () => StarkBank.Invoice.Create(starkInvoices), cancellationToken);

                var externalIds = created.Select(i => i.ID).ToList();

                _logger.LogInformation(
                    "Successfully created {Count} invoices", externalIds.Count);

                return StarkBankOperationResult<IEnumerable<string>>.Ok(externalIds);
            }
            catch (StarkBankError ex)
            {
                _logger.LogError(ex,
                    "Stark Bank API error while creating invoices. Message: {Message}",
                    ex.Message);

                return StarkBankOperationResult<IEnumerable<string>>.Fail(
                    "STARK_BANK_API_ERROR", ex.Message);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "Invoice creation operation was cancelled");

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error while creating invoices in Stark Bank");

                return StarkBankOperationResult<IEnumerable<string>>.Fail(
                    "UNEXPECTED_ERROR",
                    "Unexpected error while creating invoices");
            }
        }
    }
}
