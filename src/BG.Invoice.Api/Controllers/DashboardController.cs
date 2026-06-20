using BG.Invoice.Api.Configuration;
using BG.Invoice.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BG.Invoice.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = AuthorizationPolicies.Authenticated)]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ICurrentUser _currentUser;

    public DashboardController(IDashboardService dashboardService, ICurrentUser currentUser)
    {
        _dashboardService = dashboardService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var result = await _dashboardService.GetSummaryAsync(_currentUser.UserId, _currentUser.IsAdmin, ct);
        return Ok(result.Value);
    }
}
