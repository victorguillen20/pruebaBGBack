namespace BG.Invoice.Application.Abstractions;

public interface IJwtTokenService
{
    string GenerateToken(int userId, string userName, string role, int roleId);
}
