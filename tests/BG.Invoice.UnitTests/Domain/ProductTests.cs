using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace BG.Invoice.UnitTests.Domain;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_InitializesCorrectly()
    {
        var product = Product.Create("P001", "Brocas", 22.00m, categoryId: 1, stock: 100, cost: 10m);
        product.Code.Should().Be("P001");
        product.Name.Should().Be("Brocas");
        product.Price.Should().Be(22.00m);
        product.Stock.Should().Be(100);
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithNegativePrice_Throws()
    {
        var act = () => Product.Create("P001", "Brocas", -1m, 1);
        act.Should().Throw<BusinessRuleException>().WithMessage("*Price cannot be negative*");
    }

    [Fact]
    public void Create_WithNegativeStock_Throws()
    {
        var act = () => Product.Create("P001", "Brocas", 10m, 1, stock: -1);
        act.Should().Throw<BusinessRuleException>().WithMessage("*Stock cannot be negative*");
    }

    [Fact]
    public void DecrementStock_ReducesStock()
    {
        var product = Product.Create("P001", "Brocas", 10m, 1, stock: 100);
        product.DecrementStock(30);
        product.Stock.Should().Be(70);
    }

    [Fact]
    public void DecrementStock_BeyondAvailable_Throws()
    {
        var product = Product.Create("P001", "Brocas", 10m, 1, stock: 10);
        var act = () => product.DecrementStock(20);
        act.Should().Throw<BusinessRuleException>().WithMessage("*Insufficient stock*");
        product.Stock.Should().Be(10);
    }

    [Fact]
    public void IncrementStock_AddsToStock()
    {
        var product = Product.Create("P001", "Brocas", 10m, 1, stock: 10);
        product.IncrementStock(5);
        product.Stock.Should().Be(15);
    }
}