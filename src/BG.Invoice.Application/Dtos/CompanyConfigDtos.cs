namespace BG.Invoice.Application.Dtos;

public record CompanyConfigResponse(
    int Id,
    string CompanyName,
    string? Phone,
    string? Email,
    string? TaxId,
    decimal TaxPercent,
    string CurrencySymbol,
    string? Address,
    string? City,
    string? Region,
    string? PostalCode,
    string? LogoUrl,
    int LastInvoiceNumber
);

public record UpdateCompanyConfigRequest(
    string CompanyName,
    decimal TaxPercent,
    string CurrencySymbol,
    string? Phone,
    string? Email,
    string? TaxId,
    string? Address,
    string? City,
    string? Region,
    string? PostalCode,
    string? LogoUrl
);
