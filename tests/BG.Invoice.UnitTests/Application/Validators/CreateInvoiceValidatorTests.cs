using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Validators;
using BG.Invoice.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace BG.Invoice.UnitTests.Application.Validators;

public class CreateInvoiceValidatorTests
{
    private readonly CreateInvoiceValidator _validator = new();

    [Fact]
    public void ValidRequest_PassesValidation()
    {
        var request = new CreateInvoiceRequest(
            1, InvoiceType.Contado, null, null,
            new List<CreateInvoiceDetailRequest>
            {
                new(1, 2, 22.00m, "Brocas", "P001")
            });
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void InvalidCustomerId_FailsValidation()
    {
        var request = new CreateInvoiceRequest(
            0, InvoiceType.Contado, null, null,
            new List<CreateInvoiceDetailRequest>
            {
                new(1, 1, 10m, "Producto", "P001")
            });
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerId");
    }

    [Fact]
    public void EmptyDetails_FailsValidation()
    {
        var request = new CreateInvoiceRequest(
            1, InvoiceType.Contado, null, null, new List<CreateInvoiceDetailRequest>());
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("at least one detail"));
    }

    [Fact]
    public void InvalidDetail_FailsValidation()
    {
        var request = new CreateInvoiceRequest(
            1, InvoiceType.Contado, null, null,
            new List<CreateInvoiceDetailRequest>
            {
                new(0, 0, -1m, "", "")
            });
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
    }
}
