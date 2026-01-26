using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SB.InvoiceToTransfer.Application.Abstractions.External;
using SB.InvoiceToTransfer.Application.Interfaces.External;
using StarkBank;
using StarkBank.Error;

namespace SB.InvoiceToTransfer.Infrastructure.External.Banking
{
    public sealed class StarkBankClient : IStarkBankClient
    {
        private readonly ILogger<StarkBankClient> _logger;
        private readonly TransferBankAccountOptions _bankAccount;

        public StarkBankClient(
            ILogger<StarkBankClient> logger,
            IOptions<TransferBankAccountOptions> transferBankAccountOptions,
            IOptions<StarkBankProjectOptions> starkBankProjectOptions)
        {
            _logger = logger;
            _bankAccount = transferBankAccountOptions.Value;

            var options = starkBankProjectOptions.Value;

            var project = new Project(
               environment: options.Environment,
               privateKey: options.PrivateKey,
               id: options.ProjectId
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

        public async Task<StarkBankOperationResult<string>> CreateTransferAsync(long amount, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating transfer of {Amount} to Stark Bank account",
                    amount);

                var transfer = new Transfer(
                    amount: amount,
                    bankCode: _bankAccount.BankCode,
                    branchCode: _bankAccount.Branch,
                    accountNumber: _bankAccount.Account,
                    taxID: _bankAccount.TaxId,
                    name: _bankAccount.Name,
                    accountType: _bankAccount.AccountType);

                var created = await Task.Run(
                    () => Transfer.Create(
                        new List<Transfer> { transfer }), cancellationToken);

                var transferId = created.First().ID;

                _logger.LogInformation(
                    "Transfer created successfully. Id: {TransferId}",
                    transferId);

                return StarkBankOperationResult<string>.Ok(transferId);
            }
            catch (StarkBankError ex)
            {
                _logger.LogError(ex,
                    "Stark Bank error while creating transfer");

                return StarkBankOperationResult<string>.Fail(
                    "STARK_BANK_API_ERROR",
                    ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error while creating transfer");

                return StarkBankOperationResult<string>.Fail(
                    "UNEXPECTED_ERROR",
                    "Unexpected error while creating transfer");
            }
        }
    }
}
