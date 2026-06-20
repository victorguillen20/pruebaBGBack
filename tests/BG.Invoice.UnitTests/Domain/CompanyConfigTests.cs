using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace BG.Invoice.UnitTests.Domain;

public class CompanyConfigTests
{
    [Fact]
    public void Create_WithValidData_StartsAtZeroInvoiceNumber()
    {
        var config = CompanyConfig.Create("Sistemas BG");
        config.LastInvoiceNumber.Should().Be(0);
        config.TaxPercent.Should().Be(13m);
        config.CurrencySymbol.Should().Be("$");
        config.Id.Should().Be(1);
    }

    [Fact]
    public void NextInvoiceNumber_IncrementsCounter()
    {
        var config = CompanyConfig.Create("Sistemas BG");
        var n1 = config.NextInvoiceNumber();
        var n2 = config.NextInvoiceNumber();
        var n3 = config.NextInvoiceNumber();
        n1.Should().Be(1);
        n2.Should().Be(2);
        n3.Should().Be(3);
    }

    [Fact]
    public void Update_ChangesValues()
    {
        var config = CompanyConfig.Create("Sistemas BG");
        config.Update("New Name", 21m, "€", null, null, null, null, null, null, null, null);
        config.CompanyName.Should().Be("New Name");
        config.TaxPercent.Should().Be(21m);
        config.CurrencySymbol.Should().Be("€");
    }
}