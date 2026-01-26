using SB.InvoiceToTransfer.Application.Abstractions.External;
using SB.InvoiceToTransfer.Domain;

namespace SB.InvoiceToTransfer.Application.Interfaces.External
{
    public interface IStarkBankClient
    {
        Task<StarkBankOperationResult<IEnumerable<string>>> CreateInvoicesAsync(
            IReadOnlyCollection<Invoice> invoices,
            CancellationToken cancellationToken);
    }
}
