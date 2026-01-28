namespace SB.InvoiceToTransfer.Application.Models
{
    public sealed class InvoiceProcessingJobState
    {
        public Guid Id { get; init; }
        public DateTime StartedAt { get; init; }
        public bool IsActive { get; private set; }

        public InvoiceProcessingJobState()
        {
            Id = Guid.NewGuid();
            StartedAt = DateTime.UtcNow;
            IsActive = true;
        }

        public void Finish()
        {
            IsActive = false;
        }
    }
}
