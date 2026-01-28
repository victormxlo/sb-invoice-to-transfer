using SB.InvoiceToTransfer.Domain;
using SB.InvoiceToTransfer.Domain.Enums;

namespace SB.InvoiceToTransfer.Application.Interfaces.Repositories
{
    public interface IInvoiceRepository
    {
        Task AddAsync(Invoice invoice, CancellationToken cancellationToken);
        Task AddRangeAsync(IEnumerable<Invoice> invoices, CancellationToken cancellationToken);
        Task<Invoice?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken);
        Task<IReadOnlyList<Invoice>> GetByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken);
        Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken);
        Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken);
    }
}
