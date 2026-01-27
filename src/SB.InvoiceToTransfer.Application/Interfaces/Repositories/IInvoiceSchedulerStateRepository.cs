using SB.InvoiceToTransfer.Application.Models;

namespace SB.InvoiceToTransfer.Application.Interfaces.Repositories
{
    public interface IInvoiceSchedulerStateRepository
    {
        Task<InvoiceSchedulerState?> GetActiveAsync(CancellationToken cancellationToken);
        Task AddAsync(InvoiceSchedulerState state, CancellationToken cancellationToken);
        Task UpdateAsync(InvoiceSchedulerState state, CancellationToken cancellationToken);
    }
}
