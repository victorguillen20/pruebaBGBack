using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Validators;
using BG.Invoice.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace BG.Invoice.UnitTests.Application.Validators;

public class CreateCustomerValidatorTests
{
    private readonly CreateCustomerValidator _validator = new();

    [Fact]
    public void ValidRequest_PassesValidation()
    {
        var request = new CreateCustomerRequest("123456", "Juan Perez", CustomerType.Persona, "555123", "juan@test.com", "Calle 123", null);
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void EmptyIdentification_FailsValidation()
    {
        var request = new CreateCustomerRequest("", "Juan Perez", CustomerType.Persona, null, null, null, null);
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Identification");
    }

    [Fact]
    public void EmptyName_FailsValidation()
    {
        var request = new CreateCustomerRequest("123456", "", CustomerType.Persona, null, null, null, null);
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void NegativeCreditLimit_FailsValidation()
    {
        var request = new CreateCustomerRequest("123456", "Juan Perez", CustomerType.Persona, null, null, null, -100);
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CreditLimit");
    }
}
