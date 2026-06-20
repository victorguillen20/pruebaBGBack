using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BG.Invoice.Infrastructure.Persistence.EntityConfigurations;
public class InvoiceDetailConfiguration : IEntityTypeConfiguration<InvoiceDetail>
{
    public void Configure(EntityTypeBuilder<InvoiceDetail> builder)
    {
        builder.ToTable("InvoiceDetails");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedOnAdd();
        builder.Property(d => d.InvoiceId).IsRequired();
        builder.Property(d => d.ProductId).IsRequired();
        builder.Property(d => d.Quantity).IsRequired();
        builder.Property(d => d.UnitPrice).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(d => d.LineTotal).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(d => d.ProductNameSnapshot).IsRequired().HasMaxLength(200);
        builder.Property(d => d.ProductCodeSnapshot).IsRequired().HasMaxLength(50);
        builder.HasIndex(d => d.ProductId);
        builder.ToTable(t => t.HasCheckConstraint("CK_InvoiceDetail_Quantity_Positive", "\"Quantity\" > 0"));
        builder.ToTable(t => t.HasCheckConstraint("CK_InvoiceDetail_UnitPrice_NonNegative", "\"UnitPrice\" >= 0"));
        builder.ToTable(t => t.HasCheckConstraint("CK_InvoiceDetail_LineTotal_NonNegative", "\"LineTotal\" >= 0"));
        builder.HasOne(d => d.Invoice).WithMany(i => i.Details)
            .HasForeignKey(d => d.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(d => d.Product).WithMany(p => p.InvoiceDetails)
            .HasForeignKey(d => d.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
