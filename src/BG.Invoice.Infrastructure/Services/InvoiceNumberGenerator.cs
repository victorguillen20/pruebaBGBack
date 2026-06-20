using System.Data;
using BG.Invoice.Application.Abstractions;
using BG.Invoice.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BG.Invoice.Infrastructure.Services;

public class InvoiceNumberGenerator : IInvoiceNumberGenerator
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public InvoiceNumberGenerator(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<int> GenerateNextAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var config = await context.CompanyConfig.FirstAsync(ct);
        var newNumber = config.NextInvoiceNumber();
        await context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return newNumber;
    }
}
