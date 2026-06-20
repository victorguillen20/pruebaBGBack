using BG.Invoice.Api.Configuration;
using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BG.Invoice.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;

    public UsersController(
        IUserService userService,
        IRepository<User> userRepository,
        IRepository<Role> roleRepository)
    {
        _userService = userService;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] int? roleId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (pageSize > 100) pageSize = 100;
        var result = await _userService.SearchAsync(search, roleId, page, pageSize, true, ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _userService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status404NotFound
            });
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken ct)
    {
        var result = await _userService.CreateAsync(request, ct);
        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails
            {
                Title = "Error",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status400BadRequest
            });
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateUserRequest request, CancellationToken ct)
    {
        var result = await _userService.UpdateAsync(id, request, ct);
        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status404NotFound
            });
        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var userResult = await _userService.GetByIdAsync(id, ct);
        if (!userResult.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = userResult.ErrorMessage,
                Status = StatusCodes.Status404NotFound
            });

        var user = userResult.Value!;
        if (user.Role == "Admin")
        {
            var roles = await _roleRepository.ListAsync(r => r.Name == "Admin", ct);
            int? adminRoleId = roles.Count > 0 ? roles[0].Id : null;
            if (adminRoleId.HasValue)
            {
                var admins = await _userRepository.ListAsync(
                    u => u.IsActive && u.RoleId == adminRoleId.Value && u.Id != id, ct);
                if (admins.Count is 0)
                    throw new BusinessRuleException("Cannot delete the last active admin.");
            }
        }

        var result = await _userService.DeactivateAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status404NotFound
            });
        return NoContent();
    }
}
