using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace BG.Invoice.UnitTests.Application.Validators;

public class UpdateCompanyConfigValidatorTests
{
    private readonly UpdateCompanyConfigValidator _validator = new();

    [Fact]
    public void ValidRequest_PassesValidation()
    {
        var request = new UpdateCompanyConfigRequest(
            "Sistemas BG", 13m, "$", "555-0000", "info@bg.com", "123456",
            "Calle Principal", "Ciudad", "Region", "10000", null);
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void EmptyCompanyName_FailsValidation()
    {
        var request = new UpdateCompanyConfigRequest(
            "", 13m, "$", null, null, null, null, null, null, null, null);
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CompanyName");
    }

    [Fact]
    public void NegativeTaxPercent_FailsValidation()
    {
        var request = new UpdateCompanyConfigRequest(
            "Sistemas BG", -5m, "$", null, null, null, null, null, null, null, null);
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TaxPercent");
    }

    [Fact]
    public void TaxPercentOver100_FailsValidation()
    {
        var request = new UpdateCompanyConfigRequest(
            "Sistemas BG", 150m, "$", null, null, null, null, null, null, null, null);
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TaxPercent");
    }

    [Fact]
    public void EmptyCurrencySymbol_FailsValidation()
    {
        var request = new UpdateCompanyConfigRequest(
            "Sistemas BG", 13m, "", null, null, null, null, null, null, null, null);
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CurrencySymbol");
    }
}
