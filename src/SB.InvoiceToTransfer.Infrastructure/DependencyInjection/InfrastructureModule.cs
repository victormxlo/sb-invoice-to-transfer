using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SB.InvoiceToTransfer.Infrastructure.DependencyInjection
{
    public static class InfrastructureModule
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return services;
        }
    }
}
