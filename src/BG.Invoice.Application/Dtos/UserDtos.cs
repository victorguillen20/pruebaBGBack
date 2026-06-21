namespace BG.Invoice.Application.Dtos;

public record UserResponse(
    int Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    int RoleId,
    string Role,
    bool IsActive,
    bool MustChangePassword,
    DateTime CreatedAt
);

public record CreateUserRequest(
    string UserName,
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    int RoleId
);

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    int RoleId
);
