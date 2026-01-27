using Mapster;
using SB.InvoiceToTransfer.Application.Models;
using SB.InvoiceToTransfer.Infrastructure.Persistence.Entities;

namespace SB.InvoiceToTransfer.Infrastructure.Mappings
{
    public sealed class InvoiceSchedulerStateMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<InvoiceSchedulerState, InvoiceSchedulerStateEntity>();
            config.NewConfig<InvoiceSchedulerStateEntity, InvoiceSchedulerState>();
        }
    }
}
