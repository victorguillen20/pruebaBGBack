namespace BG.Invoice.Application.Abstractions;

public interface IInvoiceNumberGenerator
{
    Task<int> GenerateNextAsync(CancellationToken ct = default);
}
