using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BG.Invoice.Application.Pdf;

public class InvoicePdfGenerator : IInvoicePdfGenerator
{
    static InvoicePdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private readonly ICompanyConfigService _configService;

    public InvoicePdfGenerator(ICompanyConfigService configService)
    {
        _configService = configService;
    }

    public async Task<byte[]> GenerateAsync(InvoiceResponse invoice, CancellationToken ct = default)
    {
        var configResult = await _configService.GetAsync(ct);
        var config = configResult.Value!;
        return BuildPdf(invoice, config);
    }

    private static byte[] BuildPdf(InvoiceResponse invoice, CompanyConfigResponse config)
    {
        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(c => ComposeHeader(c, invoice, config));
                page.Content().Element(c => ComposeContent(c, invoice, config));
                page.Footer().Element(c => ComposeFooter(c));
            });
        }).GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, InvoiceResponse invoice, CompanyConfigResponse config)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    if (!string.IsNullOrWhiteSpace(config.LogoUrl))
                    {
                        left.Item().Text(config.LogoUrl).FontSize(8).FontColor(Colors.Grey.Medium);
                    }
                    left.Item().Text(config.CompanyName).FontSize(14).Bold().FontColor("#2c3e50");
                    if (!string.IsNullOrWhiteSpace(config.Address))
                        left.Item().Text(config.Address).FontSize(9);
                    if (!string.IsNullOrWhiteSpace(config.City))
                        left.Item().Text(config.City).FontSize(9);
                    if (!string.IsNullOrWhiteSpace(config.Phone))
                        left.Item().Text($"Teléfono: {config.Phone}").FontSize(9);
                    if (!string.IsNullOrWhiteSpace(config.Email))
                        left.Item().Text($"Email: {config.Email}").FontSize(9);
                });

                row.RelativeItem().AlignRight().Column(right =>
                {
                    right.Item().Text($"FACTURA N° {invoice.Number}").FontSize(18).Bold().FontColor("#2c3e50");
                });
            });

            col.Item().PaddingTop(5).LineHorizontal(1).LineColor("#2c3e50");
        });
    }

    private static void ComposeContent(IContainer container, InvoiceResponse invoice, CompanyConfigResponse config)
    {
        container.Column(col =>
        {
            col.Spacing(10);

            col.Item().PaddingTop(10).Element(c => ComposeCustomerSection(c, invoice));
            col.Item().Element(c => ComposeInfoSection(c, invoice));
            col.Item().Element(c => ComposeItemsTable(c, invoice, config));
            col.Item().AlignRight().Element(c => ComposeTotals(c, invoice, config));
        });
    }

    private static void ComposeCustomerSection(IContainer container, InvoiceResponse invoice)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
        {
            col.Spacing(4);
            col.Item().Text("FACTURAR A").FontSize(10).Bold().FontColor("#2c3e50");
            col.Item().Text(invoice.CustomerName).FontSize(11).Bold();
            if (!string.IsNullOrWhiteSpace(invoice.CustomerIdentification))
                col.Item().Text($"Identificación: {invoice.CustomerIdentification}").FontSize(9);
            if (!string.IsNullOrWhiteSpace(invoice.CustomerPhone))
                col.Item().Text($"Teléfono: {invoice.CustomerPhone}").FontSize(9);
            if (!string.IsNullOrWhiteSpace(invoice.CustomerEmail))
                col.Item().Text($"Email: {invoice.CustomerEmail}").FontSize(9);
        });
    }

    private static void ComposeInfoSection(IContainer container, InvoiceResponse invoice)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn();
                c.RelativeColumn();
                c.RelativeColumn();
            });

            table.Header(header =>
            {
                header.Cell().Background("#f5f5f5").Padding(6).Text("VENDEDOR").FontSize(9).Bold().FontColor("#2c3e50");
                header.Cell().Background("#f5f5f5").Padding(6).Text("FECHA").FontSize(9).Bold().FontColor("#2c3e50");
                header.Cell().Background("#f5f5f5").Padding(6).Text("FORMA DE PAGO").FontSize(9).Bold().FontColor("#2c3e50");
            });

            var paymentMethod = invoice.Payments.Count > 0
                ? invoice.Payments[0].Method.ToString()
                : "N/A";

            table.Cell().Padding(6).Text(invoice.SellerName).FontSize(9);
            table.Cell().Padding(6).Text(invoice.Date.ToString("dd/MM/yyyy")).FontSize(9);
            table.Cell().Padding(6).Text(paymentMethod).FontSize(9);
        });
    }

    private static void ComposeItemsTable(IContainer container, InvoiceResponse invoice, CompanyConfigResponse config)
    {
        var symbol = config.CurrencySymbol;

        container.Border(1).BorderColor(Colors.Grey.Lighten2).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.ConstantColumn(50);
                c.RelativeColumn();
                c.ConstantColumn(100);
                c.ConstantColumn(100);
            });

            table.Header(header =>
            {
                header.Cell().Background("#f5f5f5").Padding(6).AlignCenter().Text("CANT.").FontSize(9).Bold().FontColor("#2c3e50");
                header.Cell().Background("#f5f5f5").Padding(6).Text("DESCRIPCIÓN").FontSize(9).Bold().FontColor("#2c3e50");
                header.Cell().Background("#f5f5f5").Padding(6).AlignRight().Text("PRECIO UNIT.").FontSize(9).Bold().FontColor("#2c3e50");
                header.Cell().Background("#f5f5f5").Padding(6).AlignRight().Text("PRECIO TOTAL").FontSize(9).Bold().FontColor("#2c3e50");
            });

            foreach (var detail in invoice.Details)
            {
                table.Cell().Padding(6).AlignCenter().Text(detail.Quantity.ToString()).FontSize(9);
                table.Cell().Padding(6).Text(detail.ProductName).FontSize(9);
                table.Cell().Padding(6).AlignRight().Text($"{symbol} {detail.UnitPrice:N2}").FontSize(9);
                table.Cell().Padding(6).AlignRight().Text($"{symbol} {detail.LineTotal:N2}").FontSize(9);
            }
        });
    }

    private static void ComposeTotals(IContainer container, InvoiceResponse invoice, CompanyConfigResponse config)
    {
        var symbol = config.CurrencySymbol;

        container.Width(250).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(col =>
        {
            col.Spacing(4);
            col.Item().Row(row =>
            {
                row.RelativeItem().Text("SUBTOTAL").FontSize(10).FontColor("#2c3e50");
                row.RelativeItem().AlignRight().Text($"{symbol} {invoice.Subtotal:N2}").FontSize(10);
            });
            col.Item().Row(row =>
            {
                row.RelativeItem().Text($"IVA ({config.TaxPercent}%)").FontSize(10).FontColor("#2c3e50");
                row.RelativeItem().AlignRight().Text($"{symbol} {invoice.TaxAmount:N2}").FontSize(10);
            });
            col.Item().PaddingTop(4).LineHorizontal(1).LineColor("#2c3e50");
            col.Item().Row(row =>
            {
                row.RelativeItem().Text("TOTAL").FontSize(12).Bold().FontColor("#2c3e50");
                row.RelativeItem().AlignRight().Text($"{symbol} {invoice.Total:N2}").FontSize(12).Bold().FontColor("#2c3e50");
            });
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.AlignCenter().PaddingTop(20).Text("¡Gracias por su compra!").FontSize(11).Italic().FontColor("#2c3e50");
    }
}
