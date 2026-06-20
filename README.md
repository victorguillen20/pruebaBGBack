# BG Invoice Backend

.NET 10 Web API for the BG invoice management system.

## Status
- **PR-1** (this commit): solution scaffold, MSBuild infrastructure, empty projects. Solution builds clean with 0 warnings.
- **PR-2..7**: coming — Domain entities, Infrastructure (EF Core + SQLite), Application services, Api controllers, seed, tests.

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

The API starts on `http://localhost:5000`. (Full endpoints and Swagger UI come in PR-5.)

## License
Internal use only.
