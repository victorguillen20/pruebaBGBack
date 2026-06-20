namespace BG.Invoice.Application.Abstractions;

public interface ICurrentUser
{
    int UserId { get; }
    string? UserName { get; }
    bool IsAdmin { get; }
    int RoleId { get; }
}
