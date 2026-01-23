using MediatR;

namespace SB.InvoiceToTransfer.Application.UseCases.CreateInvoices
{
    public sealed record CreateInvoicesCommand : IRequest<CreateInvoicesResult>;
}
