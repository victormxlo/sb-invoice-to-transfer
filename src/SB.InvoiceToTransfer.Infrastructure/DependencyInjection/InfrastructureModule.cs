using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SB.InvoiceToTransfer.Application.Interfaces.External;
using SB.InvoiceToTransfer.Application.Interfaces.Repositories;
using SB.InvoiceToTransfer.Infrastructure.Background;
using SB.InvoiceToTransfer.Infrastructure.Configuration;
using SB.InvoiceToTransfer.Infrastructure.External.Banking;
using SB.InvoiceToTransfer.Infrastructure.Mappings;
using SB.InvoiceToTransfer.Infrastructure.Persistence;
using SB.InvoiceToTransfer.Infrastructure.Repositories;

namespace SB.InvoiceToTransfer.Infrastructure.DependencyInjection
{
    public static class InfrastructureModule
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            AddDatabase(services);
            AddOptions(services, configuration);
            AddExternalClients(services);
            AddRepositories(services);
            AddServices(services);
            AddMappings();

            return services;
        }

        public static void AddDatabase(
            IServiceCollection services)
        {
            services.AddDbContext<InvoiceToTransferDbContext>(options =>
            {
                options.UseSqlite(Secrets.Require("SB_DB_CONNECTION"));
            });
        }

        public static void AddOptions(
            IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<InvoiceSchedulerOptions>(
                configuration.GetSection("InvoiceScheduler"));

            services
                .AddOptions<TransferBankAccountOptions>()
                .Bind(configuration.GetSection("StarkBank:TransferAccount"))
                .ValidateOnStart();

            services
                .AddOptions<StarkBankProjectOptions>()
                .Configure(options =>
                {
                    options.Environment = Secrets.Require("SB_ENVIRONMENT");
                    options.PrivateKey = Secrets.Require("SB_PRIVATE_KEY");
                    options.ProjectId = Secrets.Require("SB_PROJECT_ID");
                })
                .ValidateOnStart();
        }

        public static void AddExternalClients(IServiceCollection services)
        {
            services.AddSingleton<IStarkBankClient, StarkBankClient>();
        }

        private static void AddRepositories(IServiceCollection services)
        {
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        }

        private static void AddServices(IServiceCollection services)
        {
            services.AddHostedService<InvoiceSchedulerService>();
        }

        private static void AddMappings()
        {
            TypeAdapterConfig.GlobalSettings.Scan(
                typeof(InvoiceSchedulerStateMapping).Assembly);
        }
    }
}
