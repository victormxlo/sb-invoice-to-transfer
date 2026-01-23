using Microsoft.EntityFrameworkCore;
using SB.InvoiceToTransfer.Domain;

namespace SB.InvoiceToTransfer.Infrastructure.Persistence
{
    public class InvoiceToTransferDbContext : DbContext
    {
        public DbSet<Invoice> Invoices => Set<Invoice>();

        public InvoiceToTransferDbContext(
            DbContextOptions<InvoiceToTransferDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .ApplyConfigurationsFromAssembly(
                    typeof(InvoiceToTransferDbContext).Assembly);
        }
    }
}
