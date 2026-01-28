namespace SB.InvoiceToTransfer.Application.UseCases.RunInvoiceProcessingJob
{
    public sealed record RunInvoiceProcessingJobResult(
        bool Executed,
        int ProcessedInvoices,
        string? Reason = null)
    {
        public static RunInvoiceProcessingJobResult Skipped(string reason)
            => new(false, 0, reason);

        public static RunInvoiceProcessingJobResult Ok(int processed)
            => new(true, processed);
    }
}
