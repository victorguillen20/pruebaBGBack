FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY global.json ./
COPY BG.Invoice.slnx ./

COPY src/BG.Invoice.Domain/BG.Invoice.Domain.csproj src/BG.Invoice.Domain/
COPY src/BG.Invoice.Application/BG.Invoice.Application.csproj src/BG.Invoice.Application/
COPY src/BG.Invoice.Infrastructure/BG.Invoice.Infrastructure.csproj src/BG.Invoice.Infrastructure/
COPY src/BG.Invoice.Api/BG.Invoice.Api.csproj src/BG.Invoice.Api/

RUN dotnet restore src/BG.Invoice.Api/BG.Invoice.Api.csproj

COPY src/BG.Invoice.Domain/ src/BG.Invoice.Domain/
COPY src/BG.Invoice.Application/ src/BG.Invoice.Application/
COPY src/BG.Invoice.Infrastructure/ src/BG.Invoice.Infrastructure/
COPY src/BG.Invoice.Api/ src/BG.Invoice.Api/

RUN dotnet publish src/BG.Invoice.Api/BG.Invoice.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "BG.Invoice.Api.dll"]
