using BG.Invoice.Application.Dtos;

namespace BG.Invoice.Application.Abstractions;

public interface IDashboardService
{
    Task<Result<DashboardSummaryResponse>> GetSummaryAsync(int requestingUserId, bool isAdmin, CancellationToken ct = default);
}
