using BG.Invoice.Application.Common;
using BG.Invoice.Application.Dtos;
using FluentValidation;

namespace BG.Invoice.Application.Validators;

public class UpdateInvoiceValidator : AbstractValidator<CreateInvoiceRequest>
{
    public UpdateInvoiceValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.Details).NotEmpty().WithMessage(Errors.Validation.InvoiceDetailsRequired);
        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.ProductId).GreaterThan(0);
            detail.RuleFor(d => d.Quantity).GreaterThan(0);
            detail.RuleFor(d => d.UnitPrice).GreaterThanOrEqualTo(0);
            detail.RuleFor(d => d.ProductName).NotEmpty().MaximumLength(200);
            detail.RuleFor(d => d.ProductCode).NotEmpty().MaximumLength(20);
        });
    }
}
