using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SB.InvoiceToTransfer.Application.Interfaces.External;
using SB.InvoiceToTransfer.Application.Interfaces.Repositories;
using SB.InvoiceToTransfer.Infrastructure.External.Banking;
using SB.InvoiceToTransfer.Infrastructure.Persistence;

namespace SB.InvoiceToTransfer.Infrastructure.DependencyInjection
{
    public static class InfrastructureModule
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<InvoiceToTransferDbContext>(options =>
            {
                var connectionString = configuration
                    .GetConnectionString("DefaultConnection");
                options.UseSqlite(connectionString);
            });

            services.AddSingleton<IStarkBankClient>(sp =>
            {
                var logger = sp
                    .GetRequiredService<ILogger<StarkBankClient>>();

                return new StarkBankClient(logger);
            });

            services.AddScoped<IInvoiceRepository, IInvoiceRepository>();

            return services;
        }
    }
}
