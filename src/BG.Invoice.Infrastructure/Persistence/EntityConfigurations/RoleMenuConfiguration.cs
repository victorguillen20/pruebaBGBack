using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BG.Invoice.Infrastructure.Persistence.EntityConfigurations;
public class RoleMenuConfiguration : IEntityTypeConfiguration<RoleMenu>
{
    public void Configure(EntityTypeBuilder<RoleMenu> builder)
    {
        builder.ToTable("RoleMenus");
        builder.HasKey(rm => new { rm.MenuId, rm.RoleId });
        builder.HasIndex(rm => rm.RoleId);
        builder.HasOne(rm => rm.Menu).WithMany(m => m.RoleMenus)
            .HasForeignKey(rm => rm.MenuId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(rm => rm.Role).WithMany(r => r.RoleMenus)
            .HasForeignKey(rm => rm.RoleId).OnDelete(DeleteBehavior.Cascade);
    }
}
