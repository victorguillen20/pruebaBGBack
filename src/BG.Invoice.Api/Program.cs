using BG.Invoice.Api.Configuration;
using BG.Invoice.Api.Extensions;
using BG.Invoice.Api.Middleware;
using BG.Invoice.Application.DI;
using BG.Invoice.Infrastructure.DI;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("Application", "BG.Invoice.Api"));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection")!);
builder.Services.AddApi(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<RequestIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors(CorsConfiguration.AllowAngularDev);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BG Invoice v1");
        c.RoutePrefix = "swagger";
    });
}

app.MapHealthChecks("/health");

app.Run();
