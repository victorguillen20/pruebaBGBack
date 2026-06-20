using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BG.Invoice.Infrastructure.Persistence.EntityConfigurations;
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();
        builder.Property(r => r.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(r => r.Name).IsUnique();
        builder.Property(r => r.Description).HasMaxLength(200);
        builder.Property(r => r.IsActive).IsRequired().HasDefaultValue(true);
        builder.HasMany(r => r.Users).WithOne(u => u.Role)
            .HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(r => r.RoleMenus).WithOne(rm => rm.Role)
            .HasForeignKey(rm => rm.RoleId).OnDelete(DeleteBehavior.Cascade);
    }
}
