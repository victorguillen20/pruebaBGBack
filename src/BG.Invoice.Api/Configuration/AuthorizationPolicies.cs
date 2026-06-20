namespace BG.Invoice.Api.Configuration;

public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string VendorOrAdmin = "VendorOrAdmin";
    public const string Authenticated = "Authenticated";

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(o =>
        {
            o.AddPolicy(AdminOnly, p => p.RequireRole("Admin"));
            o.AddPolicy(VendorOrAdmin, p => p.RequireRole("Admin", "Vendedor"));
            o.AddPolicy(Authenticated, p => p.RequireAuthenticatedUser());
        });

        return services;
    }
}
