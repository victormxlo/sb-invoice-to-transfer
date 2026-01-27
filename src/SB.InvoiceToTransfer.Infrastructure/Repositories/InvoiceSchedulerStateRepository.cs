using Mapster;
using Microsoft.EntityFrameworkCore;
using SB.InvoiceToTransfer.Application.Interfaces.Repositories;
using SB.InvoiceToTransfer.Application.Models;
using SB.InvoiceToTransfer.Infrastructure.Persistence;
using SB.InvoiceToTransfer.Infrastructure.Persistence.Entities;

namespace SB.InvoiceToTransfer.Infrastructure.Repositories
{
    public sealed class InvoiceSchedulerStateRepository : IInvoiceSchedulerStateRepository
    {
        private readonly InvoiceToTransferDbContext _context;

        public InvoiceSchedulerStateRepository(InvoiceToTransferDbContext context)
        {
            _context = context;
        }

        public async Task<InvoiceSchedulerState?> GetActiveAsync(
            CancellationToken cancellationToken)
        {
            return await _context.InvoiceSchedulerStates
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ProjectToType<InvoiceSchedulerState>()
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(
            InvoiceSchedulerState state,
            CancellationToken cancellationToken)
        {
            var entity = state.Adapt<InvoiceSchedulerStateEntity>();

            await _context.InvoiceSchedulerStates.AddAsync(
                entity,
                cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(
            InvoiceSchedulerState state,
            CancellationToken cancellationToken)
        {
            var entity = await _context.InvoiceSchedulerStates
                .FirstAsync(
                    x => x.Id == state.Id,
                    cancellationToken);

            state.Adapt(entity);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
