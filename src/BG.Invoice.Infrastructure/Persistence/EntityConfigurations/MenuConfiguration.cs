using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BG.Invoice.Infrastructure.Persistence.EntityConfigurations;
public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("Menus");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedOnAdd();
        builder.Property(m => m.Key).IsRequired().HasMaxLength(50);
        builder.HasIndex(m => m.Key).IsUnique();
        builder.Property(m => m.Label).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Icon).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Route).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Order).IsRequired().HasDefaultValue(0);
        builder.Property(m => m.IsActive).IsRequired().HasDefaultValue(true);
        builder.HasIndex(m => new { m.IsActive, m.Order });
        builder.ToTable(t => t.HasCheckConstraint("CK_Menu_Order_NonNegative", "\"Order\" >= 0"));
    }
}
