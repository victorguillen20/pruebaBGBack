using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Enums;
using BG.Invoice.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace BG.Invoice.UnitTests.Domain;

public class CustomerTests
{
    [Fact]
    public void Create_WithIdentificationWithDashes_StoresNormalized()
    {
        var customer = Customer.Create("001-0123456-7", "Test Customer", CustomerType.Persona);
        customer.Identification.Should().Be("00101234567");
    }

    [Fact]
    public void Create_WithPhoneWithDashes_StoresNormalized()
    {
        var customer = Customer.Create("00101234567", "Test Customer", CustomerType.Persona, "555-100");
        customer.Phone.Should().Be("555100");
    }

    [Fact]
    public void Create_WithPhoneLongerThan6Digits_Throws()
    {
        var act = () => Customer.Create("00101234567", "Test Customer", CustomerType.Persona, "5551007");
        act.Should().Throw<BusinessRuleException>().WithMessage("*máximo 6 dígitos*");
    }

    [Fact]
    public void Create_WithShortIdentification_Throws()
    {
        var act = () => Customer.Create("12345", "Test Customer", CustomerType.Persona);
        act.Should().Throw<BusinessRuleException>().WithMessage("*entre 10 y 13 dígitos*");
    }

    [Fact]
    public void Create_WithIdentificationWithLetters_Throws()
    {
        var act = () => Customer.Create("ID-001", "Test Customer", CustomerType.Persona);
        act.Should().Throw<BusinessRuleException>().WithMessage("*entre 10 y 13 dígitos*");
    }

    [Fact]
    public void Update_WithPhoneWithDashes_StoresNormalized()
    {
        var customer = Customer.Create("00101234567", "Test Customer", CustomerType.Persona);
        customer.Update("Updated Name", CustomerType.Empresa, "555-000", null, null, null);
        customer.Phone.Should().Be("555000");
    }

    [Fact]
    public void Update_WithPhoneLongerThan6Digits_Throws()
    {
        var customer = Customer.Create("00101234567", "Test Customer", CustomerType.Persona);
        var act = () => customer.Update("Updated Name", CustomerType.Empresa, "55500007", null, null, null);
        act.Should().Throw<BusinessRuleException>().WithMessage("*máximo 6 dígitos*");
    }

    [Fact]
    public void Create_WithNullPhone_DoesNotThrow()
    {
        var customer = Customer.Create("00101234567", "Test Customer", CustomerType.Persona, null);
        customer.Phone.Should().BeNull();
    }

    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var customer = Customer.Create("00101234567", "Test Customer", CustomerType.Persona, "555100", "test@test.com", "123 Main St", 1000m);
        customer.Identification.Should().Be("00101234567");
        customer.Name.Should().Be("Test Customer");
        customer.Type.Should().Be(CustomerType.Persona);
        customer.Phone.Should().Be("555100");
        customer.Email.Should().Be("test@test.com");
        customer.Address.Should().Be("123 Main St");
        customer.CreditLimit.Should().Be(1000m);
        customer.IsActive.Should().BeTrue();
    }
}
