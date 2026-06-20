using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BG.Invoice.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRepository<Role> _repository;
    private readonly ILogger<RoleService> _logger;

    public RoleService(IRepository<Role> repository, ILogger<RoleService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RoleResponse>> ListAsync(CancellationToken ct = default)
    {
        var roles = await _repository.ListAsync(r => r.IsActive, ct);
        return roles.Select(r => new RoleResponse(r.Id, r.Name, r.Description)).ToList();
    }
}
