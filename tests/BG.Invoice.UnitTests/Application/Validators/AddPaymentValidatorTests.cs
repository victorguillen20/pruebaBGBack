using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Validators;
using BG.Invoice.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace BG.Invoice.UnitTests.Application.Validators;

public class AddPaymentValidatorTests
{
    private readonly AddPaymentValidator _validator = new();

    [Fact]
    public void ValidRequest_PassesValidation()
    {
        var request = new AddPaymentRequest(PaymentMethod.Efectivo, 100m, "Recibo #123", DateTime.UtcNow);
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ZeroAmount_FailsValidation()
    {
        var request = new AddPaymentRequest(PaymentMethod.Efectivo, 0m, null, null);
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Amount");
    }

    [Fact]
    public void NegativeAmount_FailsValidation()
    {
        var request = new AddPaymentRequest(PaymentMethod.Efectivo, -50m, null, null);
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Amount");
    }
}
