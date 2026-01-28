using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SB.InvoiceToTransfer.Infrastructure.Configuration;

namespace SB.InvoiceToTransfer.Infrastructure.Persistence
{
    public class InvoiceToTransferDbContextFactory : IDesignTimeDbContextFactory<InvoiceToTransferDbContext>
    {
        public InvoiceToTransferDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<InvoiceToTransferDbContext>();

            optionsBuilder.UseSqlite($"Data Source={Secrets.Require("SB_DB_CONNECTION")}");

            return new InvoiceToTransferDbContext(optionsBuilder.Options);
        }
    }
}
