using BG.Invoice.Application.Abstractions;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Infrastructure.Auth;
using BG.Invoice.Infrastructure.Clock;
using BG.Invoice.Infrastructure.Persistence;
using BG.Invoice.Infrastructure.Persistence.Interceptors;
using BG.Invoice.Infrastructure.Persistence.Repositories;
using BG.Invoice.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BG.Invoice.Infrastructure.DI;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddHttpContextAccessor();
        services.TryAddSingleton<AuditSaveChangesInterceptor>();
        services.AddScoped<AppDbContext>(sp =>
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connectionString)
                .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>())
                .Options;
            return new AppDbContext(options);
        });
        services.AddDbContextFactory<AppDbContext>(opt =>
            opt.UseSqlite(connectionString), ServiceLifetime.Singleton);

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<global::BG.Invoice.Infrastructure.Persistence.Repositories.IUnitOfWork, UnitOfWork>();
        services.AddScoped<global::BG.Invoice.Application.Abstractions.IUnitOfWork>(sp =>
            (global::BG.Invoice.Application.Abstractions.IUnitOfWork)sp.GetRequiredService<global::BG.Invoice.Infrastructure.Persistence.Repositories.IUnitOfWork>());

        services.AddScoped(typeof(global::BG.Invoice.Application.Abstractions.IRepository<>), typeof(Repository<>));

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IClock, SystemClock>();
        services.AddScoped<IInvoiceNumberGenerator, InvoiceNumberGenerator>();

        return services;
    }
}
