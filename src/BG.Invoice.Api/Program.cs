using BG.Invoice.Application.DI;
using BG.Invoice.Infrastructure.DI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection")!);

var app = builder.Build();
app.MapGet("/", () => "BG Invoice API");
app.Run();
