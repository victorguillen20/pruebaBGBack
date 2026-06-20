using BG.Invoice.Api.Configuration;
using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BG.Invoice.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUser _currentUser;

    public AuthController(IAuthService authService, ICurrentUser currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = string.Join("; ", result.ValidationErrors),
                Status = StatusCodes.Status400BadRequest
            });
        return Ok(result.Value);
    }

    [Authorize(Policy = AuthorizationPolicies.Authenticated)]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        var result = await _authService.ChangePasswordAsync(request, _currentUser.UserId, ct);
        if (!result.IsSuccess)
        {
            if (result.ValidationErrors.Count > 0)
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = string.Join("; ", result.ValidationErrors),
                    Status = StatusCodes.Status400BadRequest
                });
            return BadRequest(new ProblemDetails
            {
                Title = "Bad Request",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status400BadRequest
            });
        }
        return Ok();
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request, ct);
        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = string.Join("; ", result.ValidationErrors),
                Status = StatusCodes.Status400BadRequest
            });
        return StatusCode(StatusCodes.Status201Created);
    }
}
