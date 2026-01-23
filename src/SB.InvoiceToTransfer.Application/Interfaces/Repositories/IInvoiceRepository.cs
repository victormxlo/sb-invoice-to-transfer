using SB.InvoiceToTransfer.Domain;

namespace SB.InvoiceToTransfer.Application.Interfaces.Repositories
{
    public interface IInvoiceRepository
    {
        Task AddAsync(Invoice invoice, CancellationToken cancellationToken);
    }
}
