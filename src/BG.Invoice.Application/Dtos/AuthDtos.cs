namespace BG.Invoice.Application.Dtos;

public record LoginRequest(string UserName, string Password);

public record LoginResponse(
    string Token,
    UserInfo User
);

public record UserInfo(
    int Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    int RoleId,
    bool MustChangePassword
);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);

public record RegisterRequest(
    string UserName,
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    int RoleId
);
