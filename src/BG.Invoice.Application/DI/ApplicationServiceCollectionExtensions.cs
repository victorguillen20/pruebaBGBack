using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BG.Invoice.Application.DI;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Result>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<ICompanyConfigService, CompanyConfigService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IRoleService, RoleService>();

        return services;
    }
}
