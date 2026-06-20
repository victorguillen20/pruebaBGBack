using BG.Invoice.Application.Dtos;

namespace BG.Invoice.Application.Abstractions;

public interface IRoleService
{
    Task<IReadOnlyList<RoleResponse>> ListAsync(CancellationToken ct = default);
}
