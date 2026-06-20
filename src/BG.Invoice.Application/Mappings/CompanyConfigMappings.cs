using BG.Invoice.Application.Dtos;
using BG.Invoice.Domain.Entities;

namespace BG.Invoice.Application.Mappings;

public static class CompanyConfigMappings
{
    public static CompanyConfigResponse ToResponse(this CompanyConfig config)
    {
        return new CompanyConfigResponse(
            config.Id, config.CompanyName, config.Phone, config.Email,
            config.TaxId, config.TaxPercent, config.CurrencySymbol,
            config.Address, config.City, config.Region,
            config.PostalCode, config.LogoUrl, config.LastInvoiceNumber);
    }
}
