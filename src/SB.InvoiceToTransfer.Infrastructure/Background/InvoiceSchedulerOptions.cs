namespace SB.InvoiceToTransfer.Infrastructure.Background
{
    public sealed class InvoiceSchedulerOptions
    {
        public TimeSpan Interval { get; init; }
    }
}
