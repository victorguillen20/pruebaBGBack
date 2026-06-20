using System.Text.Json.Serialization;
using BG.Invoice.Api.Auth;
using BG.Invoice.Api.Configuration;
using BG.Invoice.Api.Filters;
using BG.Invoice.Api.HealthChecks;
using BG.Invoice.Application.Abstractions;

namespace BG.Invoice.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICurrentUser, CurrentUserService>();

        services.AddProblemDetails();

        services.AddHealthChecks()
            .AddCheck<DbContextHealthCheck>("db");

        services.AddCorsPolicies();
        services.AddJwtBearerAuthentication(configuration);
        services.AddAuthorizationPolicies();
        services.AddSwaggerWithJwt();

        services.AddControllers(o =>
        {
            o.Filters.Add<ValidationActionFilter>();
        })
        .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }
}
