using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SB.InvoiceToTransfer.Infrastructure.Persistence
{
    public class InvoiceToTransferDbContextFactory : IDesignTimeDbContextFactory<InvoiceToTransferDbContext>
    {
        public InvoiceToTransferDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<InvoiceToTransferDbContext>();

            optionsBuilder.UseSqlite(
                "Data Source=invoice_to_transfer.db"
            );

            return new InvoiceToTransferDbContext(optionsBuilder.Options);
        }
    }
}
