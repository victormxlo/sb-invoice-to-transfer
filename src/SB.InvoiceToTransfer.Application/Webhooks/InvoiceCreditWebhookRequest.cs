namespace SB.InvoiceToTransfer.Application.Webhooks
{
    public sealed class InvoiceCreditWebhookRequest
    {
        public string Event { get; init; } = default!;
        public InvoiceCreditEventDto Data { get; init; } = default!;
    }
}
