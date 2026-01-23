using SB.InvoiceToTransfer.Domain.Enums;

namespace SB.InvoiceToTransfer.Domain
{
    public class Invoice
    {
        public Guid Id { get; private set; }
        public string ExternalId { get; private set; }
        public decimal Amount { get; private set; }
        public InvoiceStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public Invoice(string externalId, decimal amount)
        {
            Id = Guid.NewGuid();
            ExternalId = externalId;
            Amount = amount;
            Status = InvoiceStatus.Created;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsPaid()
        {
            Status = InvoiceStatus.Paid;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsTransferred()
        {
            Status = InvoiceStatus.Transferred;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
