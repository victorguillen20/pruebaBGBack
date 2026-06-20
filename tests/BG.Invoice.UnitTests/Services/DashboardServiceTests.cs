using System.Linq.Expressions;
using System.Reflection;
using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Common;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Services;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace BG.Invoice.UnitTests.Services;

public class DashboardServiceTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepository = new();
    private readonly Mock<IRepository<Customer>> _customerRepository = new();
    private readonly Mock<IRepository<Product>> _productRepository = new();
    private readonly ILogger<DashboardService> _logger = Mock.Of<ILogger<DashboardService>>();
    private readonly DashboardService _sut;

    public DashboardServiceTests()
    {
        _sut = new DashboardService(_invoiceRepository.Object, _customerRepository.Object, _productRepository.Object, _logger);
    }

    private static BG.Invoice.Domain.Entities.Invoice CreateInvoice(int id, int number, int sellerId, decimal total, InvoiceStatus status)
    {
        var invoice = BG.Invoice.Domain.Entities.Invoice.Create(number, DateTime.UtcNow, 1, sellerId, InvoiceType.Contado);
        typeof(BG.Invoice.Domain.Entities.Invoice).GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(invoice, id);
        typeof(BG.Invoice.Domain.Entities.Invoice).GetField("<Date>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(invoice, DateTime.UtcNow);
        typeof(BG.Invoice.Domain.Entities.Invoice).GetField("<Total>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(invoice, total);
        typeof(BG.Invoice.Domain.Entities.Invoice).GetField("<Status>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(invoice, status);
        return invoice;
    }

    [Fact]
    public async Task GetSummaryAsync_Admin_ReturnsFullSummary()
    {
        var invoices = new List<BG.Invoice.Domain.Entities.Invoice>
        {
            CreateInvoice(1, 1001, 20, 100m, InvoiceStatus.Pagada),
            CreateInvoice(2, 1002, 21, 200m, InvoiceStatus.Pendiente)
        };
        var recentInvoices = new List<BG.Invoice.Domain.Entities.Invoice> { invoices[0] };
        var topProducts = new List<TopProductRow>
        {
            new(1, "P001", "Product A", 10, 500m)
        };
        var customers = new List<Customer>
        {
            Customer.Create("ID-001", "Customer A", CustomerType.Persona)
        };
        var products = new List<Product>
        {
            Product.Create("P001", "Product A", 10m, 1, 100)
        };

        _invoiceRepository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<BG.Invoice.Domain.Entities.Invoice, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoices.AsReadOnly());
        _invoiceRepository.Setup(r => r.GetRecentAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentInvoices.AsReadOnly());
        _invoiceRepository.Setup(r => r.GetTopProductsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topProducts.AsReadOnly());
        _customerRepository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<Customer, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers.AsReadOnly());
        _productRepository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products.AsReadOnly());

        var result = await _sut.GetSummaryAsync(1, true);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalInvoices.Should().Be(2);
        result.Value.TotalCustomers.Should().Be(1);
        result.Value.TotalProducts.Should().Be(1);
        result.Value.TotalActiveProducts.Should().Be(1);
        result.Value.TotalRevenue.Should().Be(300m);
        result.Value.PendingAmount.Should().Be(300m);
        result.Value.PaidAmount.Should().Be(0m);
        result.Value.DraftInvoices.Should().Be(1);
        result.Value.PaidInvoices.Should().Be(1);
        result.Value.CancelledInvoices.Should().Be(0);
        result.Value.RecentInvoices.Should().HaveCountLessThanOrEqualTo(5);
        result.Value.TopProducts.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetSummaryAsync_Vendor_ReturnsFilteredSummary()
    {
        var adminInvoice = CreateInvoice(1, 1001, 1, 50m, InvoiceStatus.Pagada);
        var vendorInvoice = CreateInvoice(2, 1002, 20, 100m, InvoiceStatus.Pagada);
        var allInvoices = new List<BG.Invoice.Domain.Entities.Invoice> { adminInvoice, vendorInvoice };

        _invoiceRepository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<BG.Invoice.Domain.Entities.Invoice, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(allInvoices.AsReadOnly());
        _invoiceRepository.Setup(r => r.GetRecentAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BG.Invoice.Domain.Entities.Invoice>().AsReadOnly());
        _invoiceRepository.Setup(r => r.GetTopProductsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TopProductRow>().AsReadOnly());
        _customerRepository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<Customer, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer>().AsReadOnly());
        _productRepository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>().AsReadOnly());

        var result = await _sut.GetSummaryAsync(20, false);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalInvoices.Should().Be(1);
    }
}
