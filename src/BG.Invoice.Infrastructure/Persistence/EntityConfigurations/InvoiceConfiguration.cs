using BG.Invoice.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BG.Invoice.Infrastructure.Persistence.EntityConfigurations;
public class InvoiceConfiguration : IEntityTypeConfiguration<global::BG.Invoice.Domain.Entities.Invoice>
{
    public void Configure(EntityTypeBuilder<global::BG.Invoice.Domain.Entities.Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedOnAdd();
        builder.Property(i => i.Number).IsRequired();
        builder.HasIndex(i => i.Number).IsUnique();
        builder.Property(i => i.Date).IsRequired().HasColumnType("TEXT");
        builder.Property(i => i.CustomerId).IsRequired();
        builder.Property(i => i.SellerId).IsRequired();
        builder.Property(i => i.Type).IsRequired().HasConversion<int>();
        builder.Property(i => i.DueDate).HasColumnType("TEXT");
        builder.Property(i => i.Status).IsRequired().HasConversion<int>().HasDefaultValue(InvoiceStatus.Pendiente);
        builder.Property(i => i.Notes).HasMaxLength(1000);
        builder.Property(i => i.Subtotal).IsRequired().HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        builder.Property(i => i.TaxAmount).IsRequired().HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        builder.Property(i => i.Total).IsRequired().HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        builder.HasIndex(i => new { i.SellerId, i.Date });
        builder.HasIndex(i => new { i.CustomerId, i.Date });
        builder.HasIndex(i => i.Date);
        builder.HasIndex(i => new { i.Status, i.Date });
        builder.HasIndex(i => i.Total);
        builder.HasIndex("CreatedAt");
        builder.ToTable(t => t.HasCheckConstraint("CK_Invoice_Number_Positive", "\"Number\" > 0"));
        builder.ToTable(t => t.HasCheckConstraint("CK_Invoice_Subtotal_NonNegative", "\"Subtotal\" >= 0"));
        builder.ToTable(t => t.HasCheckConstraint("CK_Invoice_TaxAmount_NonNegative", "\"TaxAmount\" >= 0"));
        builder.ToTable(t => t.HasCheckConstraint("CK_Invoice_Total_NonNegative", "\"Total\" >= 0"));
        builder.HasOne(i => i.Customer).WithMany(c => c.Invoices)
            .HasForeignKey(i => i.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.Seller).WithMany(u => u.InvoicesAsSeller)
            .HasForeignKey(i => i.SellerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(i => i.Details).WithOne(d => d.Invoice)
            .HasForeignKey(d => d.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(i => i.Payments).WithOne(p => p.Invoice)
            .HasForeignKey(p => p.InvoiceId).OnDelete(DeleteBehavior.Cascade);
    }
}
