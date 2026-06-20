using BG.Invoice.Application.Dtos;
using FluentValidation;

namespace BG.Invoice.Application.Validators;

public class AddPaymentValidator : AbstractValidator<AddPaymentRequest>
{
    public AddPaymentValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reference).MaximumLength(100).When(x => x.Reference != null);
    }
}
