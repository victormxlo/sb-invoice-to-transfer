namespace SB.InvoiceToTransfer.Infrastructure.External.Banking
{
    public sealed class TransferBankAccountOptions
    {
        public string BankCode { get; init; } = "20018183";
        public string Branch { get; init; } = "0001";
        public string Account { get; init; } = "6341320293482496";
        public string Name { get; init; } = "Stark Bank S.A.";
        public string TaxId { get; init; } = "20.018.183/0001-80";
        public string AccountType { get; init; } = "payment";
    }
}
