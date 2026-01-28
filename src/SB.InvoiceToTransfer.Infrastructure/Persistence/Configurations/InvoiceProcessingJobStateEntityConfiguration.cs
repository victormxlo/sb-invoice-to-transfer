using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.InvoiceToTransfer.Infrastructure.Persistence.Entities;

namespace SB.InvoiceToTransfer.Infrastructure.Persistence.Configurations
{
    public class InvoiceProcessingJobStateEntityConfiguration : IEntityTypeConfiguration<InvoiceProcessingJobStateEntity>
    {
        public void Configure(EntityTypeBuilder<InvoiceProcessingJobStateEntity> builder)
        {
            builder.ToTable("InvoiceProcessingJobStates");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.StartedAt)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.HasIndex(x => x.IsActive)
                .HasFilter("IsActive = 1")
                .IsUnique();
        }
    }
}
