namespace SB.InvoiceToTransfer.Application.Webhooks
{
    public sealed class InvoiceDto
    {
        public string Id { get; init; } = default!;
        public long Amount { get; init; }
        public long Fee { get; init; }
        public string Status { get; init; } = default!;
    }
}
