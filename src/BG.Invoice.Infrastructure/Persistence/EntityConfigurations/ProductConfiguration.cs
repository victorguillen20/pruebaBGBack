using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BG.Invoice.Infrastructure.Persistence.EntityConfigurations;
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();
        builder.Property(p => p.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(p => p.Code).IsUnique();
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Price).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(p => p.Cost).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Stock).IsRequired().HasDefaultValue(0);
        builder.Property(p => p.CategoryId).IsRequired();
        builder.Property(p => p.IsActive).IsRequired().HasDefaultValue(true);
        builder.HasIndex(p => new { p.CategoryId, p.IsActive, p.Name });
        builder.HasIndex(p => new { p.IsActive, p.Name });
        builder.ToTable(t => t.HasCheckConstraint("CK_Product_Stock_NonNegative", "\"Stock\" >= 0"));
        builder.ToTable(t => t.HasCheckConstraint("CK_Product_Price_NonNegative", "\"Price\" >= 0"));
        builder.ToTable(t => t.HasCheckConstraint("CK_Product_Cost_NonNegative", "\"Cost\" IS NULL OR \"Cost\" >= 0"));
        builder.HasOne(p => p.Category).WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}
