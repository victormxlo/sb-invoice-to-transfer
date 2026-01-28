using MediatR;

namespace SB.InvoiceToTransfer.Application.UseCases.RunInvoiceProcessingJob
{
    public sealed record RunInvoiceProcessingJobCommand : IRequest<RunInvoiceProcessingJobResult>;
}
