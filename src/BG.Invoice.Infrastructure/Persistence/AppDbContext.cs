using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace BG.Invoice.Infrastructure.Persistence;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<RoleMenu> RoleMenus => Set<RoleMenu>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<global::BG.Invoice.Domain.Entities.Invoice> Invoices => Set<global::BG.Invoice.Domain.Entities.Invoice>();
    public DbSet<InvoiceDetail> InvoiceDetails => Set<InvoiceDetail>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<CompanyConfig> CompanyConfig => Set<CompanyConfig>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IAuditable).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).Property<DateTime>("CreatedAt")
                    .IsRequired()
                    .HasColumnType("TEXT")
                    .HasDefaultValueSql("datetime('now')")
                    .ValueGeneratedOnAdd();
                modelBuilder.Entity(entityType.ClrType).Property<int>("CreatedBy").IsRequired();
                modelBuilder.Entity(entityType.ClrType).Property<DateTime?>("ModifiedAt")
                    .HasColumnType("TEXT");
                modelBuilder.Entity(entityType.ClrType).Property<int?>("ModifiedBy");
            }
        }
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
