using BG.Invoice.Application.Dtos;

namespace BG.Invoice.Application.Abstractions;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<Result> ChangePasswordAsync(ChangePasswordRequest request, int userId, CancellationToken ct = default);
    Task<Result> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
}
