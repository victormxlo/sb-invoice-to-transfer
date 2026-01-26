using SB.InvoiceToTransfer.Domain.Enums;

namespace SB.InvoiceToTransfer.Domain
{
    public class Invoice
    {
        public Guid Id { get; private set; }
        public string ExternalId { get; private set; }
        public string RecipientName { get; private set; }
        public decimal Amount { get; private set; }
        public decimal? AmountPaid { get; private set; }
        public string TaxId { get; private set; }
        public string Email { get; private set; }
        public InvoiceStatus Status { get; private set; }
        public DateTime DueDate { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public DateTime? PaidAt { get; private set; }

        public Invoice(
            string recipientName, decimal amount,
            string taxId, string email,
            DateTime dueDate)
        {
            Id = Guid.NewGuid();
            RecipientName = recipientName;
            Amount = amount;
            TaxId = taxId;
            Email = email;
            DueDate = dueDate;
            Status = InvoiceStatus.Created;
            CreatedAt = DateTime.UtcNow;
        }

        public void AssignExternalId(string externalId)
            => ExternalId = externalId;

        public void MarkAsPaid(decimal amountPaid)
        {
            AmountPaid = amountPaid;
            Status = InvoiceStatus.Paid;
            UpdatedAt = DateTime.UtcNow;
            PaidAt = DateTime.UtcNow;
        }

        public void MarkAsTransferred()
        {
            Status = InvoiceStatus.Transferred;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
