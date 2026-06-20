using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Mappings;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace BG.Invoice.Application.Services;

public class UserService : IUserService
{
    private readonly IRepository<User> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(IRepository<User> repository, IUnitOfWork unitOfWork, ILogger<UserService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UserResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _repository.GetByIdAsync(id, ct);
        if (user is null)
            return Result.Failure<UserResponse>($"User with id '{id}' was not found.");
        var createdAt = DateTime.UtcNow;
        return Result.Success(user.ToResponse(user.Role?.Name ?? "", createdAt));
    }

    public async Task<PagedResult<UserResponse>> SearchAsync(string? search, int? roleId, int page, int pageSize, bool activeOnly = true, CancellationToken ct = default)
    {
        var all = await _repository.ListAsync(u =>
            (!activeOnly || u.IsActive) &&
            (!roleId.HasValue || u.RoleId == roleId) &&
            (string.IsNullOrWhiteSpace(search) ||
             u.UserName.Contains(search) || u.Email.Contains(search) ||
             u.FirstName.Contains(search) || u.LastName.Contains(search)), ct);
        var total = all.Count;
        var items = all.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => u.ToResponse(u.Role?.Name ?? "", DateTime.UtcNow)).ToList();
        return new PagedResult<UserResponse> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<Result<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var hash = "";
        var user = User.Create(request.UserName, request.Email, hash, request.FirstName, request.LastName, request.RoleId);
        await _repository.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        var createdAt = DateTime.UtcNow;
        return Result.Success(user.ToResponse(user.Role?.Name ?? "", createdAt));
    }

    public async Task<Result<UserResponse>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _repository.GetByIdAsync(id, ct);
        if (user is null)
            return Result.Failure<UserResponse>($"User with id '{id}' was not found.");
        var createdAt = DateTime.UtcNow;
        return Result.Success(user.ToResponse(user.Role?.Name ?? "", createdAt));
    }

    public async Task<Result> DeactivateAsync(int id, CancellationToken ct = default)
    {
        var user = await _repository.GetByIdAsync(id, ct);
        if (user is null)
            return Result.Failure($"User with id '{id}' was not found.");
        user.Deactivate();
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ActivateAsync(int id, CancellationToken ct = default)
    {
        var user = await _repository.GetByIdAsync(id, ct);
        if (user is null)
            return Result.Failure($"User with id '{id}' was not found.");
        user.Activate();
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
