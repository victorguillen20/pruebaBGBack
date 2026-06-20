using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace BG.Invoice.UnitTests.Application.Validators;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    [Fact]
    public void ValidRequest_PassesValidation()
    {
        var request = new CreateProductRequest("P001", "Brocas", 22.00m, 1, 100, 10m, "Brocas de acero");
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void EmptyCode_FailsValidation()
    {
        var request = new CreateProductRequest("", "Brocas", 22.00m, 1, 0, null, null);
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code");
    }

    [Fact]
    public void NegativePrice_FailsValidation()
    {
        var request = new CreateProductRequest("P001", "Brocas", -1m, 1, 0, null, null);
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Fact]
    public void InvalidCategoryId_FailsValidation()
    {
        var request = new CreateProductRequest("P001", "Brocas", 22.00m, 0, 0, null, null);
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CategoryId");
    }
}
