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
    private readonly DbConnection _connection;
    private bool _seeded;

    public CustomWebApplicationFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
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

            services.AddSingleton<DbConnection>(_connection);

            services.AddScoped<AppDbContext>(sp =>
            {
                var conn = sp.GetRequiredService<DbConnection>();
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite(conn)
                    .Options;
                return new AppDbContext(options);
            });
        });
    }

    public async Task SeedAsync()
    {
        if (_seeded)
            return;
        _seeded = true;

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var roles = new List<Role>
        {
            Role.Create("Admin", "Full system access"),
            Role.Create("Vendedor", "Sales access, can only see own invoices")
        };
        roles[0].Id = 1;
        roles[1].Id = 2;
        db.Set<Role>().AddRange(roles);
        await db.SaveChangesAsync();

        var menus = new List<Menu>
        {
            Menu.Create("dashboard", "Dashboard", "dashboard", "/dashboard", 1),
            Menu.Create("invoices", "Invoices", "receipt", "/invoices", 2),
            Menu.Create("customers", "Customers", "people", "/customers", 3),
            Menu.Create("products", "Products", "inventory", "/products", 4),
            Menu.Create("users", "Users", "group", "/users", 5),
            Menu.Create("config", "Configuration", "settings", "/config", 6)
        };
        for (int i = 0; i < menus.Count; i++)
            menus[i].Id = i + 1;
        db.Set<Menu>().AddRange(menus);
        await db.SaveChangesAsync();

        var roleMenus = new List<RoleMenu>
        {
            new(1, 1), new(2, 1), new(3, 1), new(4, 1), new(5, 1), new(6, 1),
            new(1, 2), new(2, 2), new(3, 2), new(4, 2)
        };
        db.Set<RoleMenu>().AddRange(roleMenus);
        await db.SaveChangesAsync();

        var hash = hasher.Hash("Admin123!");
        var users = new List<User>
        {
            User.Create("admin", "admin@bg.com", hash, "System", "Admin", 1),
            User.Create("vendor1", "vendor1@bg.com", hash, "Maria", "Gonzalez", 2),
            User.Create("vendor2", "vendor2@bg.com", hash, "Carlos", "Ramirez", 2)
        };
        for (int i = 0; i < users.Count; i++)
            users[i].Id = i + 1;
        db.Set<User>().AddRange(users);
        await db.SaveChangesAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
            _seeded = false;
        }
        base.Dispose(disposing);
    }
}
