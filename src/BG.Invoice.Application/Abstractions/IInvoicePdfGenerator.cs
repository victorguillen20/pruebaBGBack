using BG.Invoice.Application.Dtos;

namespace BG.Invoice.Application.Abstractions;

public interface IInvoicePdfGenerator
{
    Task<byte[]> GenerateAsync(InvoiceResponse invoice, CancellationToken ct = default);
}
