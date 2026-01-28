using Microsoft.EntityFrameworkCore;
using SB.InvoiceToTransfer.Application.Interfaces.Repositories;
using SB.InvoiceToTransfer.Domain;
using SB.InvoiceToTransfer.Domain.Enums;
using SB.InvoiceToTransfer.Infrastructure.Persistence;

namespace SB.InvoiceToTransfer.Infrastructure.Repositories
{
    public sealed class InvoiceRepository : IInvoiceRepository
    {
        private readonly InvoiceToTransferDbContext _context;

        public InvoiceRepository(InvoiceToTransferDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken)
        {
            await _context.Invoices.AddAsync(invoice, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<Invoice> invoices, CancellationToken cancellationToken)
        {
            await _context.Invoices.AddRangeAsync(invoices, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Invoice?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken)
        {
            return await _context.Invoices
                .FirstOrDefaultAsync(x => x.ExternalId == externalId, cancellationToken);
        }

        public async Task<IReadOnlyList<Invoice>> GetByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken)
        {
            return await _context.Invoices
                .Where(x => x.Status == status).ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken)
        {
            return await _context.Invoices
                .AnyAsync(x => x.ExternalId == externalId, cancellationToken);
        }

        public async Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken)
        {
            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
