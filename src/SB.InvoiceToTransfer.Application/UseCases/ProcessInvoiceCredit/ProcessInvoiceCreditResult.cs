namespace SB.InvoiceToTransfer.Application.UseCases.ProcessInvoiceCredit
{
    public sealed record ProcessInvoiceCreditResult
    {
        public bool Success { get; }
        public string? TransferId { get; }
        public string? Reason { get; }

        public ProcessInvoiceCreditResult(bool success, string? transferId, string? reason)
        {
            Success = success;
            TransferId = transferId;
            Reason = reason;
        }

        public static ProcessInvoiceCreditResult Ok(string transferId)
            => new(true, transferId, null);

        public static ProcessInvoiceCreditResult AlreadyProcessed()
            => new(false, null, "Already processed");

        public static ProcessInvoiceCreditResult NotProcessed(string reason)
            => new(false, null, reason);
    }
}
