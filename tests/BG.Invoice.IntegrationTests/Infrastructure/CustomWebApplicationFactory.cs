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
