using SB.InvoiceToTransfer.Application.Models;

namespace SB.InvoiceToTransfer.Application.Interfaces.Repositories
{
    public interface IInvoiceProcessingJobStateRepository
    {
        Task<InvoiceProcessingJobState?> GetActiveAsync(CancellationToken cancellationToken);
        Task AddAsync(InvoiceProcessingJobState state, CancellationToken cancellationToken);
        Task UpdateAsync(InvoiceProcessingJobState state, CancellationToken cancellationToken);
    }
}
