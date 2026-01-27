namespace SB.InvoiceToTransfer.Infrastructure.Persistence.Entities
{
    public sealed class InvoiceSchedulerStateEntity
    {
        public Guid Id { get; private set; }
        public DateTime StartedAt { get; private set; }
        public bool IsActive { get; private set; }

        private InvoiceSchedulerStateEntity() { }

        public InvoiceSchedulerStateEntity(
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
