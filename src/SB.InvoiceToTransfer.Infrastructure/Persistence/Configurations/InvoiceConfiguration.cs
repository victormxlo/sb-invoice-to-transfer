using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.InvoiceToTransfer.Domain;

namespace SB.InvoiceToTransfer.Infrastructure.Persistence.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoices");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.ExternalId)
                .HasMaxLength(300)
                .IsRequired();

            builder.Property(x => x.RecipientName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Amount)
                .IsRequired();

            builder.Property(x => x.AmountPaid)
                .IsRequired(false);

            builder.Property(x => x.TaxId)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.TransferId)
                .HasMaxLength(300);

            builder.Property(x => x.Email)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt);

            builder.Property(x => x.PaidAt);
        }
    }
}
