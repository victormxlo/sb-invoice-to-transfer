namespace SB.InvoiceToTransfer.Application.Webhooks
{
    public sealed class InvoiceCreditEventDto
    {
        public InvoiceDto Invoice { get; init; } = default!;
    }
}
