using BG.Invoice.Api.Configuration;
using BG.Invoice.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BG.Invoice.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var roles = await _roleService.ListAsync(ct);
        return Ok(roles);
    }
}
