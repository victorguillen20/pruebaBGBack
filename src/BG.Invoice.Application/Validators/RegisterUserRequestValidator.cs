using BG.Invoice.Application.Common;
using BG.Invoice.Application.Dtos;
using FluentValidation;

namespace BG.Invoice.Application.Validators;

public class RegisterUserRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RoleId).GreaterThan(0);
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage(Errors.Validation.PasswordsDoNotMatch);
    }
}
