using BG.Invoice.Api.Configuration;
using BG.Invoice.Application.Abstractions;
using BG.Invoice.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BG.Invoice.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class RolesController : ControllerBase
{
    private readonly IRepository<Role> _roleRepository;

    public RolesController(IRepository<Role> roleRepository)
    {
        _roleRepository = roleRepository;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var roles = await _roleRepository.ListAsync(r => r.IsActive, ct);
        var response = roles.Select(r => new RoleResponse(r.Id, r.Name, r.Description)).ToList();
        return Ok(response);
    }
}

public record RoleResponse(int Id, string Name, string? Description);
