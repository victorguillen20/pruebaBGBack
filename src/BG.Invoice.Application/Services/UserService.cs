using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Common;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Mappings;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace BG.Invoice.Application.Services;

public class UserService : IUserService
{
    private const string AdminRoleName = "Admin";

    private readonly IUserRepository _repository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository repository,
        IRepository<Role> roleRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<UserService> logger)
    {
        _repository = repository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UserResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _repository.GetByIdWithRoleAsync(id, ct);
        if (user is null)
            throw new NotFoundException("User", id);
        var createdAt = DateTime.UtcNow;
        return Result.Success(user.ToResponse(user.Role?.Name ?? "", createdAt));
    }

    public async Task<PagedResult<UserResponse>> SearchAsync(string? search, int? roleId, int page, int pageSize, bool activeOnly = true, CancellationToken ct = default)
    {
        var (items, total) = await _repository.SearchPagedAsync(search, roleId, page, pageSize, activeOnly, ct);
        var responses = items
            .Select(u => u.ToResponse(u.Role?.Name ?? "", DateTime.UtcNow))
            .ToList();
        return new PagedResult<UserResponse> { Items = responses, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<Result<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var existingUsername = await _repository.ListAsync(u => u.UserName == request.UserName, ct);
        if (existingUsername.Any())
            return Result.Failure<UserResponse>(Errors.User.UsernameTaken);

        var existingEmail = await _repository.ListAsync(u => u.Email == request.Email, ct);
        if (existingEmail.Any())
            return Result.Failure<UserResponse>(Errors.User.EmailRegistered);

        var hash = _passwordHasher.Hash(request.Password);
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
            throw new NotFoundException("User", id);

        user.UpdateProfile(request.FirstName, request.LastName, request.RoleId);
        _repository.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);

        var createdAt = DateTime.UtcNow;
        return Result.Success(user.ToResponse(user.Role?.Name ?? "", createdAt));
    }

    public async Task<Result> DeactivateAsync(int id, CancellationToken ct = default)
    {
        var user = await _repository.GetByIdAsync(id, ct);
        if (user is null)
            throw new NotFoundException("User", id);

        await EnsureNotLastActiveAdminAsync(user, ct);

        user.Deactivate();
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ActivateAsync(int id, CancellationToken ct = default)
    {
        var user = await _repository.GetByIdAsync(id, ct);
        if (user is null)
            throw new NotFoundException("User", id);
        user.Activate();
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task EnsureNotLastActiveAdminAsync(User user, CancellationToken ct)
    {
        var adminRoles = await _roleRepository.ListAsync(r => r.Name == AdminRoleName, ct);
        if (adminRoles.Count is 0)
            return;

        var adminRoleId = adminRoles[0].Id;
        if (user.RoleId != adminRoleId || !user.IsActive)
            return;

        var otherAdmins = await _repository.ListAsync(
            u => u.IsActive && u.RoleId == adminRoleId && u.Id != user.Id, ct);
        if (otherAdmins.Count is 0)
            throw new BusinessRuleException(Errors.User.LastActiveAdmin);
    }
}
