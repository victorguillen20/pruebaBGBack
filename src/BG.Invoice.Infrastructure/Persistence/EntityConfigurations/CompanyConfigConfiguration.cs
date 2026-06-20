using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BG.Invoice.Infrastructure.Persistence.EntityConfigurations;
public class CompanyConfigConfiguration : IEntityTypeConfiguration<CompanyConfig>
{
    public void Configure(EntityTypeBuilder<CompanyConfig> builder)
    {
        builder.ToTable("CompanyConfig");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd().HasDefaultValue(1);
        builder.Property(c => c.CompanyName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.Email).HasMaxLength(255);
        builder.Property(c => c.TaxId).HasMaxLength(50);
        builder.Property(c => c.TaxPercent).IsRequired().HasColumnType("decimal(5,2)").HasDefaultValue(13m);
        builder.Property(c => c.CurrencySymbol).IsRequired().HasMaxLength(10).HasDefaultValue("$");
        builder.Property(c => c.Address).HasMaxLength(500);
        builder.Property(c => c.City).HasMaxLength(100);
        builder.Property(c => c.Region).HasMaxLength(100);
        builder.Property(c => c.PostalCode).HasMaxLength(20);
        builder.Property(c => c.LogoUrl).HasMaxLength(500);
        builder.Property(c => c.LastInvoiceNumber).IsRequired().HasDefaultValue(0);
        builder.ToTable(t => t.HasCheckConstraint("CK_CompanyConfig_TaxPercent_NonNegative", "\"TaxPercent\" >= 0"));
        builder.ToTable(t => t.HasCheckConstraint("CK_CompanyConfig_LastInvoiceNumber_NonNegative", "\"LastInvoiceNumber\" >= 0"));
    }
}
