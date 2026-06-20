using BG.Invoice.Infrastructure.Persistence;
using BG.Invoice.Infrastructure.Persistence.Interceptors;
using BG.Invoice.Infrastructure.Persistence.Repositories;
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
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
