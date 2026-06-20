using Microsoft.EntityFrameworkCore.Design;
namespace BG.Invoice.Infrastructure.Persistence;
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=bg-invoice-design.db")
            .Options;
        return new AppDbContext(options);
    }
}
