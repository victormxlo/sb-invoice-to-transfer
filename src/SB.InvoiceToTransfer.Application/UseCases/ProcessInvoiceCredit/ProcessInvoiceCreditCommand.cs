using MediatR;

namespace SB.InvoiceToTransfer.Application.UseCases.ProcessInvoiceCredit
{
    public sealed record ProcessInvoiceCreditCommand : IRequest<ProcessInvoiceCreditResult>
    {
        public string InvoiceExternalId { get; set; } = default!;
        public long Amount { get; set; }
        public long Fee { get; set; }
    }
}
