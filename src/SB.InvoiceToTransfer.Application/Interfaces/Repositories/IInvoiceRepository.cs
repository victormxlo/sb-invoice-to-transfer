using SB.InvoiceToTransfer.Domain;

namespace SB.InvoiceToTransfer.Application.Interfaces.Repositories
{
    public interface IInvoiceRepository
    {
        Task AddAsync(Invoice invoice, CancellationToken cancellationToken);
        Task AddRangeAsync(IEnumerable<Invoice> invoices, CancellationToken cancellationToken);
        Task<Invoice?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken);
        Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken);
        Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken);
    }
}
