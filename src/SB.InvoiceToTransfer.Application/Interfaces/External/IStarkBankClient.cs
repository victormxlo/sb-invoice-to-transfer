namespace SB.InvoiceToTransfer.Application.Interfaces.External
{
    public interface IStarkBankClient
    {
        Task<string> CreateInvoiceAsync(
            string name,
            string taxId,
            decimal amount,
            DateTime dueDate,
            CancellationToken cancellationToken);
    }
}
