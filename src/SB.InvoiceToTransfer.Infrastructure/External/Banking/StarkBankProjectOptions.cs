namespace SB.InvoiceToTransfer.Infrastructure.External.Banking
{
    public sealed class StarkBankProjectOptions
    {
        public string Environment { get; set; } = default!;
        public string PrivateKey { get; set; } = default!;
        public string ProjectId { get; set; } = default!;
    }
}
