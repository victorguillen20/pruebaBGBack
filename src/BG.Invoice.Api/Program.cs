var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/", () => "BG Invoice API");
app.Run();
