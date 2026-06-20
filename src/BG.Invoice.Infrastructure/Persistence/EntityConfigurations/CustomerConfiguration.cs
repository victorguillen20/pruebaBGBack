using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BG.Invoice.Infrastructure.Persistence.EntityConfigurations;
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();
        builder.Property(c => c.Identification).IsRequired().HasMaxLength(50);
        builder.HasIndex(c => c.Identification).IsUnique();
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(c => c.Name);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.Email).HasMaxLength(255);
        builder.Property(c => c.Address).HasMaxLength(500);
        builder.Property(c => c.Type).IsRequired().HasConversion<int>();
        builder.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(c => c.CreditLimit).HasColumnType("decimal(18,2)");
        builder.HasIndex(c => new { c.IsActive, c.Name });
        builder.ToTable(t => t.HasCheckConstraint("CK_Customer_CreditLimit_NonNegative",
            "\"CreditLimit\" IS NULL OR \"CreditLimit\" >= 0"));
    }
}
