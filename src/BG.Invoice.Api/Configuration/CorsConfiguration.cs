namespace BG.Invoice.Api.Configuration;

public static class CorsConfiguration
{
    public const string AllowAngularDev = "AllowAngularDev";

    public static IServiceCollection AddCorsPolicies(this IServiceCollection services)
    {
        var allowedOrigins = new List<string> { "http://localhost:4200" };

        var prodOrigins = Environment.GetEnvironmentVariable("Cors__AllowedOrigins");
        if (!string.IsNullOrWhiteSpace(prodOrigins))
        {
            foreach (var origin in prodOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = origin.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                    allowedOrigins.Add(trimmed);
            }
        }

        services.AddCors(o => o.AddPolicy(AllowAngularDev, p =>
            p.WithOrigins(allowedOrigins.ToArray())
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));

        return services;
    }
}
