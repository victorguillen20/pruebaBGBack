using BG.Invoice.Application.Abstractions;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BG.Invoice.Api.HostedServices;

public class SeedHostedService : IHostedService
{
    public const string DefaultPassword = "Admin123!";

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SeedHostedService> _logger;

    public SeedHostedService(IServiceProvider serviceProvider, ILogger<SeedHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await SeedAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database seeding failed. Application will continue without seed data.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;
        var dbContext = sp.GetRequiredService<AppDbContext>();
        var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
        var seedData = sp.GetRequiredService<ISeedDataProvider>();

        await dbContext.Database.MigrateAsync(ct);

        var existingUsers = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(dbContext.Set<User>(), ct);
        if (existingUsers.Count > 0)
        {
            _logger.LogInformation("Database already seeded. Skipping seed.");

            var admin = existingUsers.FirstOrDefault(u => u.UserName == "admin");
            if (admin is not null && !admin.IsActive)
            {
                admin.Activate();
                await dbContext.SaveChangesAsync(ct);
                _logger.LogInformation("Admin user reactivated after accidental soft-delete.");
            }

            return;
        }

        _logger.LogInformation("Seeding database with default data. Default credentials: admin / {DefaultPassword}", DefaultPassword);

        var roles = seedData.GetDefaultRoles();
        await dbContext.Set<Role>().AddRangeAsync(roles, ct);
        await dbContext.SaveChangesAsync(ct);

        var menus = seedData.GetDefaultMenus();
        await dbContext.Set<Menu>().AddRangeAsync(menus, ct);
        await dbContext.SaveChangesAsync(ct);

        var roleMenus = seedData.GetDefaultRoleMenus();
        await dbContext.Set<RoleMenu>().AddRangeAsync(roleMenus, ct);
        await dbContext.SaveChangesAsync(ct);

        var passwordHash = passwordHasher.Hash(DefaultPassword);
        var users = seedData.GetDefaultUsers(passwordHash);
        await dbContext.Set<User>().AddRangeAsync(users, ct);
        await dbContext.SaveChangesAsync(ct);

        var categories = seedData.GetDefaultCategories();
        await dbContext.Set<Category>().AddRangeAsync(categories, ct);
        await dbContext.SaveChangesAsync(ct);

        var products = seedData.GetDefaultProducts();
        await dbContext.Set<Product>().AddRangeAsync(products, ct);
        await dbContext.SaveChangesAsync(ct);

        var customers = seedData.GetDefaultCustomers();
        await dbContext.Set<Customer>().AddRangeAsync(customers, ct);
        await dbContext.SaveChangesAsync(ct);

        var adminUser = users[0];
        var invoices = seedData.GetDefaultInvoices();
        await dbContext.Set<Domain.Entities.Invoice>().AddRangeAsync(invoices, ct);
        await dbContext.SaveChangesAsync(ct);

        var companyConfig = seedData.GetDefaultCompanyConfig();
        await dbContext.Set<CompanyConfig>().AddRangeAsync(new[] { companyConfig }, ct);
        await dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Database seeded successfully.");
    }
}
