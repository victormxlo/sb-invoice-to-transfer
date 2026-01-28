namespace SB.InvoiceToTransfer.Infrastructure.Background
{
    public sealed class InvoiceProcessingJobOptions
    {
        public TimeSpan Interval { get; init; }
    }
}
