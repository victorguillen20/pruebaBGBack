using BG.Invoice.Application.Dtos;
using FluentValidation;

namespace BG.Invoice.Application.Validators;

public class UpdateCompanyConfigValidator : AbstractValidator<UpdateCompanyConfigRequest>
{
    public UpdateCompanyConfigValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TaxPercent).InclusiveBetween(0, 100);
        RuleFor(x => x.CurrencySymbol).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Phone).MaximumLength(20).When(x => x.Phone != null);
        RuleFor(x => x.Email).MaximumLength(100).EmailAddress().When(x => x.Email != null);
        RuleFor(x => x.TaxId).MaximumLength(20).When(x => x.TaxId != null);
        RuleFor(x => x.Address).MaximumLength(500).When(x => x.Address != null);
        RuleFor(x => x.City).MaximumLength(100).When(x => x.City != null);
        RuleFor(x => x.Region).MaximumLength(100).When(x => x.Region != null);
        RuleFor(x => x.PostalCode).MaximumLength(20).When(x => x.PostalCode != null);
        RuleFor(x => x.LogoUrl).MaximumLength(500).When(x => x.LogoUrl != null);
    }
}
