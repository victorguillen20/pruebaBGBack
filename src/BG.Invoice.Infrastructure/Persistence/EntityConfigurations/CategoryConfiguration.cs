using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BG.Invoice.Infrastructure.Persistence.EntityConfigurations;
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(c => c.Name).IsUnique();
        builder.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);
        builder.HasIndex(c => c.IsActive);
        builder.HasMany(c => c.Products).WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}
