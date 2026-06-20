# BG Invoice Backend

.NET 10 Web API

## Stack
- .NET 10 (C# 14)
- Clean Architecture (Domain / Application / Infrastructure / Api)
- EF Core 10 + SQLite
- JWT Bearer auth (built-in `PasswordHasher<User>`)
- Serilog (console + file)
- FluentValidation
- Swashbuckle (Swagger)
- HealthChecks

## Run

```bash
# Restore
dotnet restore

# Build
dotnet build

# Run the API
dotnet run --project src/BG.Invoice.Api
```

The API starts on `http://localhost:5000`.
