using BG.Invoice.Application.Common;
using BG.Invoice.Application.Dtos;
using FluentValidation;

namespace BG.Invoice.Application.Validators;

public class UpdateInvoiceValidator : AbstractValidator<CreateInvoiceRequest>
{
    public UpdateInvoiceValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Details).NotEmpty().WithMessage(Errors.Validation.InvoiceDetailsRequired);
        RuleForEach(x => x.Details).ChildRules(d =>
        {
            d.RuleFor(x => x.ProductId).GreaterThan(0);
            d.RuleFor(x => x.Quantity).GreaterThan(0);
            d.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
