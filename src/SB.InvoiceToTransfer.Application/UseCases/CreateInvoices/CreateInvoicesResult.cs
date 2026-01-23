namespace SB.InvoiceToTransfer.Application.UseCases.CreateInvoices
{
    public sealed record CreateInvoicesResult(
        int Quantity,
        IReadOnlyCollection<string> ExternalInvoiceIds,
        DateTime ExecutedAt
    );
}
