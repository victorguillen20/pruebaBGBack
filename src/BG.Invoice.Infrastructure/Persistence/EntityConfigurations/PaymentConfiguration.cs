using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BG.Invoice.Infrastructure.Persistence.EntityConfigurations;
public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();
        builder.Property(p => p.InvoiceId).IsRequired();
        builder.Property(p => p.Method).IsRequired().HasConversion<int>();
        builder.Property(p => p.Amount).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(p => p.Reference).HasMaxLength(200);
        builder.Property(p => p.PaymentDate).IsRequired().HasColumnType("TEXT");
        builder.HasIndex(p => new { p.InvoiceId, p.PaymentDate });
        builder.HasIndex(p => new { p.Method, p.PaymentDate });
        builder.ToTable(t => t.HasCheckConstraint("CK_Payment_Amount_Positive", "\"Amount\" > 0"));
        builder.HasOne(p => p.Invoice).WithMany(i => i.Payments)
            .HasForeignKey(p => p.InvoiceId).OnDelete(DeleteBehavior.Cascade);
    }
}
