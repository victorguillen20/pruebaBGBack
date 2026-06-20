using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace BG.Invoice.Infrastructure.Persistence.EntityConfigurations;
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedOnAdd();
        builder.Property(u => u.UserName).IsRequired().HasMaxLength(50);
        builder.HasIndex(u => u.UserName).IsUnique();
        builder.Property(u => u.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.RoleId).IsRequired();
        builder.HasIndex(u => new { u.RoleId, u.IsActive });
        builder.Property(u => u.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(u => u.MustChangePassword).IsRequired().HasDefaultValue(true);
        builder.Property(u => u.FailedLoginAttempts).IsRequired().HasDefaultValue(0);
        builder.Property(u => u.LockoutUntil);
        builder.HasOne(u => u.Role).WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(u => u.InvoicesAsSeller).WithOne(i => i.Seller)
            .HasForeignKey(i => i.SellerId).OnDelete(DeleteBehavior.Restrict);
    }
}
