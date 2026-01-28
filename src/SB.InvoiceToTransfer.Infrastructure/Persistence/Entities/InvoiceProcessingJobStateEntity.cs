namespace SB.InvoiceToTransfer.Infrastructure.Persistence.Entities
{
    public sealed class InvoiceProcessingJobStateEntity
    {
        public Guid Id { get; private set; }
        public DateTime StartedAt { get; private set; }
        public bool IsActive { get; private set; }

        private InvoiceProcessingJobStateEntity() { }

        public InvoiceProcessingJobStateEntity(
            Guid id,
            DateTime startedAt,
            bool isActive)
        {
            Id = id;
            StartedAt = startedAt;
            IsActive = isActive;
        }
    }
}
