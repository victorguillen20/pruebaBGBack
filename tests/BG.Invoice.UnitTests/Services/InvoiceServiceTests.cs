using System.Linq.Expressions;
using System.Reflection;
using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Common;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Services;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Enums;
using BG.Invoice.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace BG.Invoice.UnitTests.Services;

public class InvoiceServiceTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepository = new();
    private readonly Mock<IRepository<Customer>> _customerRepository = new();
    private readonly Mock<IRepository<Product>> _productRepository = new();
    private readonly Mock<IInvoiceNumberGenerator> _numberGenerator = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IClock> _clock = new();
    private readonly ILogger<InvoiceService> _logger = Mock.Of<ILogger<InvoiceService>>();
    private readonly InvoiceService _sut;

    public InvoiceServiceTests()
    {
        _sut = new InvoiceService(
            _invoiceRepository.Object,
            _customerRepository.Object,
            _productRepository.Object,
            _numberGenerator.Object,
            _unitOfWork.Object,
            _clock.Object,
            _logger);
    }

    private static BG.Invoice.Domain.Entities.Invoice CreateTestInvoice(int id, int number, int customerId, int sellerId)
    {
        var invoice = BG.Invoice.Domain.Entities.Invoice.Create(number, DateTime.UtcNow, customerId, sellerId, InvoiceType.Contado);
        typeof(BG.Invoice.Domain.Entities.Invoice).GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(invoice, id);
        typeof(BG.Invoice.Domain.Entities.Invoice).GetField("<Date>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(invoice, DateTime.UtcNow);
        return invoice;
    }

    private static Customer CreateCustomer(int id)
    {
        var customer = Customer.Create("ID-001", "Test Customer", CustomerType.Persona);
        typeof(Customer).GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(customer, id);
        return customer;
    }

    private static Product CreateProduct(int id, int stock = 100)
    {
        var product = Product.Create("P001", "Test Product", 22.00m, 1, stock);
        typeof(Product).GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(product, id);
        return product;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingInvoice_ReturnsInvoiceResponse()
    {
        var invoice = CreateTestInvoice(1, 1001, 10, 20);
        invoice.AddDetail(1, 2, 22.00m, "Producto", "P001");

        _invoiceRepository.Setup(r => r.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var result = await _sut.GetByIdAsync(1, 20, false);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(1);
        result.Value.Number.Should().Be(1001);
        result.Value.SellerId.Should().Be(20);
        _invoiceRepository.Verify(r => r.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentInvoice_ThrowsNotFoundException()
    {
        _invoiceRepository.Setup(r => r.GetByIdWithDetailsAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BG.Invoice.Domain.Entities.Invoice?)null);

        var act = async () => await _sut.GetByIdAsync(99, 1, true);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Invoice*");
    }

    [Fact]
    public async Task GetByIdAsync_NonAdminWrongSeller_ThrowsForbiddenException()
    {
        var invoice = CreateTestInvoice(1, 1001, 10, 999);

        _invoiceRepository.Setup(r => r.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var act = async () => await _sut.GetByIdAsync(1, 1, false);

        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage(Errors.Invoice.AccessDenied);
    }

    [Fact]
    public async Task CreateAsync_ProductNotFound_ThrowsNotFoundException()
    {
        var request = new CreateInvoiceRequest(
            10, InvoiceType.Contado, null, null,
            new List<CreateInvoiceDetailRequest>
            {
                new(99, 2, 22.00m, "Producto", "P001")
            });

        _customerRepository.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCustomer(10));
        _numberGenerator.Setup(g => g.GenerateNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1001);
        _clock.SetupGet(c => c.UtcNow).Returns(DateTime.UtcNow);
        _productRepository.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var act = async () => await _sut.CreateAsync(request, 20);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Product*");
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesInvoice()
    {
        var customer = CreateCustomer(10);
        var product = CreateProduct(1, 50);
        var request = new CreateInvoiceRequest(
            10, InvoiceType.Contado, null, null,
            new List<CreateInvoiceDetailRequest>
            {
                new(1, 2, 22.00m, "Producto", "P001")
            });

        _customerRepository.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _numberGenerator.Setup(g => g.GenerateNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1001);
        _clock.SetupGet(c => c.UtcNow).Returns(DateTime.UtcNow);
        _productRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _sut.CreateAsync(request, 20);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Number.Should().Be(1001);
        product.Stock.Should().Be(48);
        _invoiceRepository.Verify(r => r.AddAsync(It.IsAny<BG.Invoice.Domain.Entities.Invoice>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ExistingInvoice_CancelsInvoice()
    {
        var invoice = CreateTestInvoice(1, 1001, 10, 20);

        _invoiceRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var result = await _sut.CancelAsync(1, 1);

        result.IsSuccess.Should().BeTrue();
        invoice.Status.Should().Be(InvoiceStatus.Anulada);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_AlreadyCancelled_ThrowsBusinessRuleException()
    {
        var invoice = CreateTestInvoice(1, 1001, 10, 20);
        invoice.Cancel();

        _invoiceRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var act = async () => await _sut.CancelAsync(1, 1);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage(Errors.Invoice.AlreadyCancelled);
    }

    [Fact]
    public async Task CancelAsync_NonExistentInvoice_ThrowsNotFoundException()
    {
        _invoiceRepository.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BG.Invoice.Domain.Entities.Invoice?)null);

        var act = async () => await _sut.CancelAsync(99, 1);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Invoice*");
    }
}
