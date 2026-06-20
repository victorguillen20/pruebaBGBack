using System.Data.Common;
using BG.Invoice.Application.Abstractions;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BG.Invoice.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath;
    private readonly string _connectionString;
    private DbConnection? _initConnection;
    private bool _seeded;

    public CustomWebApplicationFactory()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"bg-invoice-tests-{Guid.NewGuid():N}.db");
        _connectionString = $"DataSource={_dbPath};Cache=Shared;Pooling=True;Foreign Keys=True;Default Timeout=30";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var seedHosted = services.FirstOrDefault(d =>
                d.ServiceType == typeof(IHostedService) &&
                d.ImplementationType?.Name == "SeedHostedService");
            if (seedHosted is not null)
                services.Remove(seedHosted);

            var dbContextDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(AppDbContext));
            if (dbContextDescriptor is not null)
                services.Remove(dbContextDescriptor);

            services.AddScoped<AppDbContext>(_ =>
            {
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite(_connectionString)
                    .Options;
                return new AppDbContext(options);
            });

            services.AddDbContextFactory<AppDbContext>(opt =>
                opt.UseSqlite(_connectionString), ServiceLifetime.Singleton);
        });
    }

    public async Task SeedAsync()
    {
        if (_seeded)
            return;
        _seeded = true;

        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var pragma = conn.CreateCommand();
        pragma.CommandText = "PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000;";
        pragma.ExecuteNonQuery();
        _initConnection = conn;

        using var scope = Services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<AppDbContext>();
        var seed = sp.GetRequiredService<ISeedDataProvider>();
        var hasher = sp.GetRequiredService<IPasswordHasher>();

        db.Database.EnsureCreated();

        var roles = seed.GetDefaultRoles();
        roles[0].Id = 1;
        roles[1].Id = 2;
        db.Set<Role>().AddRange(roles);
        await db.SaveChangesAsync();

        var menus = seed.GetDefaultMenus();
        for (var i = 0; i < menus.Count; i++)
            menus[i].Id = i + 1;
        db.Set<Menu>().AddRange(menus);
        await db.SaveChangesAsync();

        db.Set<RoleMenu>().AddRange(seed.GetDefaultRoleMenus());
        await db.SaveChangesAsync();

        var hash = hasher.Hash("Admin123!");
        var users = seed.GetDefaultUsers(hash);
        for (var i = 0; i < users.Count; i++)
            users[i].Id = i + 1;
        db.Set<User>().AddRange(users);
        await db.SaveChangesAsync();

        var categories = seed.GetDefaultCategories();
        for (var i = 0; i < categories.Count; i++)
            categories[i].Id = i + 1;
        db.Set<Category>().AddRange(categories);
        await db.SaveChangesAsync();

        var products = seed.GetDefaultProducts();
        for (var i = 0; i < products.Count; i++)
            products[i].Id = i + 1;
        db.Set<Product>().AddRange(products);
        await db.SaveChangesAsync();

        var customers = seed.GetDefaultCustomers();
        for (var i = 0; i < customers.Count; i++)
            customers[i].Id = i + 1;
        db.Set<Customer>().AddRange(customers);
        await db.SaveChangesAsync();

        var config = seed.GetDefaultCompanyConfig();
        config.Id = 1;
        db.Set<CompanyConfig>().Add(config);
        await db.SaveChangesAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try { _initConnection?.Close(); } catch { }
            try { _initConnection?.Dispose(); } catch { }
            _seeded = false;
            try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { }
            try { if (File.Exists(_dbPath + "-wal")) File.Delete(_dbPath + "-wal"); } catch { }
            try { if (File.Exists(_dbPath + "-shm")) File.Delete(_dbPath + "-shm"); } catch { }
        }
        base.Dispose(disposing);
    }
}
