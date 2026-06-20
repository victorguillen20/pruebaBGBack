using BG.Invoice.Application.Dtos;

namespace BG.Invoice.Application.Abstractions;

public interface IUserService
{
    Task<Result<UserResponse>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<UserResponse>> SearchAsync(string? search, int? roleId, int page, int pageSize, bool activeOnly = true, CancellationToken ct = default);
    Task<Result<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<Result<UserResponse>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default);
    Task<Result> DeactivateAsync(int id, CancellationToken ct = default);
    Task<Result> ActivateAsync(int id, CancellationToken ct = default);
}
