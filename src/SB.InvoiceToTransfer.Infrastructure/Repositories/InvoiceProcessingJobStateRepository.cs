using Mapster;
using Microsoft.EntityFrameworkCore;
using SB.InvoiceToTransfer.Application.Interfaces.Repositories;
using SB.InvoiceToTransfer.Application.Models;
using SB.InvoiceToTransfer.Infrastructure.Persistence;
using SB.InvoiceToTransfer.Infrastructure.Persistence.Entities;

namespace SB.InvoiceToTransfer.Infrastructure.Repositories
{
    public sealed class InvoiceProcessingJobStateRepository : IInvoiceProcessingJobStateRepository
    {
        private readonly InvoiceToTransferDbContext _context;

        public InvoiceProcessingJobStateRepository(InvoiceToTransferDbContext context)
        {
            _context = context;
        }

        public async Task<InvoiceProcessingJobState?> GetActiveAsync(
            CancellationToken cancellationToken)
        {
            return await _context.InvoiceProcessingJobStates
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ProjectToType<InvoiceProcessingJobState>()
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(
            InvoiceProcessingJobState state,
            CancellationToken cancellationToken)
        {
            var entity = state.Adapt<InvoiceProcessingJobStateEntity>();

            await _context.InvoiceProcessingJobStates.AddAsync(
                entity,
                cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(
            InvoiceProcessingJobState state,
            CancellationToken cancellationToken)
        {
            var entity = await _context.InvoiceProcessingJobStates
                .FirstAsync(x => x.Id == state.Id, cancellationToken);

            entity.SetIsActive(state.IsActive);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
