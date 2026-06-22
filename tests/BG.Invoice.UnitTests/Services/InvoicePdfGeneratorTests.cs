using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Pdf;
using BG.Invoice.Application.Services;
using FluentAssertions;
using Moq;

namespace BG.Invoice.UnitTests.Services;

public class InvoicePdfGeneratorTests
{
    private readonly Mock<ICompanyConfigService> _configService = new();
    private readonly InvoicePdfGenerator _sut;

    public InvoicePdfGeneratorTests()
    {
        _configService.Setup(s => s.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<CompanyConfigResponse>(new CompanyConfigResponse(
                1, "BG Invoice Demo", "+593 555-123", "info@bg.com", "3-101-123456", 13m, "$",
                "Avenida Central, Edificio 1", "San Jose", "Central", "10101", null, 0)));
        _sut = new InvoicePdfGenerator(_configService.Object);
    }

    private static InvoiceResponse BuildSampleInvoice()
    {
        return new InvoiceResponse(
            1, 100, new DateTime(2026, 6, 21, 12, 0, 0, DateTimeKind.Utc),
            10, "Cliente Test", "00101234567", "555100", "cliente@test.com",
            20, "Vendedor Test", InvoiceType.Contado, null, InvoiceStatus.Pagada,
            null, 22.00m, 2.86m, 24.86m,
            new List<InvoiceDetailResponse>
            {
                new(1, 5, "Producto Test", "P001", 1, 22.00m, 22.00m)
            },
            new List<PaymentResponse>
            {
                new(1, PaymentMethod.Efectivo, 24.86m, null, new DateTime(2026, 6, 21, 12, 0, 0, DateTimeKind.Utc))
            },
            new DateTime(2026, 6, 21, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task GenerateAsync_ReturnsValidPdfBytes()
    {
        var invoice = BuildSampleInvoice();

        var result = await _sut.GenerateAsync(invoice);

        result.Should().NotBeNullOrEmpty();
        var header = System.Text.Encoding.ASCII.GetString(result, 0, 4);
        header.Should().Be("%PDF");
    }

    [Fact]
    public async Task GenerateAsync_ContainsInvoiceNumber()
    {
        var invoice = BuildSampleInvoice();

        var result = await _sut.GenerateAsync(invoice);

        var text = System.Text.Encoding.UTF8.GetString(result);
        text.Should().Contain("FACTURA N");
    }

    [Fact]
    public async Task GenerateAsync_ContainsCustomerName()
    {
        var invoice = BuildSampleInvoice();

        var result = await _sut.GenerateAsync(invoice);

        var text = System.Text.Encoding.UTF8.GetString(result);
        text.Should().Contain("Cliente Test");
    }

    [Fact]
    public async Task GenerateAsync_ContainsCompanyName()
    {
        var invoice = BuildSampleInvoice();

        var result = await _sut.GenerateAsync(invoice);

        var text = System.Text.Encoding.UTF8.GetString(result);
        text.Should().Contain("BG Invoice Demo");
    }

    [Fact]
    public async Task GenerateAsync_ContainsTotals()
    {
        var invoice = BuildSampleInvoice();

        var result = await _sut.GenerateAsync(invoice);

        var text = System.Text.Encoding.UTF8.GetString(result);
        text.Should().Contain("SUBTOTAL");
        text.Should().Contain("IVA");
        text.Should().Contain("TOTAL");
    }
}
