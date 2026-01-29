using Mapster;
using SB.InvoiceToTransfer.Application.Models;
using SB.InvoiceToTransfer.Infrastructure.Persistence.Entities;

namespace SB.InvoiceToTransfer.Infrastructure.Mappings
{
    public sealed class InvoiceProcessingJobStateMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<InvoiceProcessingJobState, InvoiceProcessingJobStateEntity>()
                .Map(dest => dest.IsActive, src => src.IsActive);
            config.NewConfig<InvoiceProcessingJobStateEntity, InvoiceProcessingJobState>();
        }
    }
}
