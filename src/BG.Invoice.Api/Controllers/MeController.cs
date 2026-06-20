using BG.Invoice.Api.Configuration;
using BG.Invoice.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BG.Invoice.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize(Policy = AuthorizationPolicies.Authenticated)]
public class MeController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMenuService _menuService;
    private readonly ICurrentUser _currentUser;

    public MeController(IUserService userService, IMenuService menuService, ICurrentUser currentUser)
    {
        _userService = userService;
        _menuService = menuService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var result = await _userService.GetByIdAsync(_currentUser.UserId, ct);
        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status404NotFound
            });
        return Ok(result.Value);
    }

    [HttpGet("menus")]
    public async Task<IActionResult> GetMyMenus(CancellationToken ct)
    {
        var menus = await _menuService.GetMenusForRoleAsync(_currentUser.RoleId, ct);
        return Ok(menus);
    }
}
