using System.Security.Claims;
using BG.Invoice.Application.Abstractions;

namespace BG.Invoice.Api.Auth;

public class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            var sub = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(sub, out var id) ? id : 0;
        }
    }

    public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

    public bool IsAdmin => _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") == true;

    public int RoleId
    {
        get
        {
            var roleIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("roleId")?.Value;
            return int.TryParse(roleIdClaim, out var id) ? id : 0;
        }
    }
}
