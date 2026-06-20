namespace BG.Invoice.Api.Configuration;

public static class CorsConfiguration
{
    public const string AllowAngularDev = "AllowAngularDev";

    public static IServiceCollection AddCorsPolicies(this IServiceCollection services)
    {
        services.AddCors(o => o.AddPolicy(AllowAngularDev, p =>
            p.WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));

        return services;
    }
}
