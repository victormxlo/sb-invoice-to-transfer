namespace SB.InvoiceToTransfer.Infrastructure.Background
{
    public sealed class InvoiceIssuanceSchedulerOptions
    {
        public TimeSpan Interval { get; init; }
    }
}
