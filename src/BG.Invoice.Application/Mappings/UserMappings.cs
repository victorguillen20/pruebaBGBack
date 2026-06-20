using BG.Invoice.Application.Dtos;
using BG.Invoice.Domain.Entities;

namespace BG.Invoice.Application.Mappings;

public static class UserMappings
{
    public static UserResponse ToResponse(this User user, string role, DateTime createdAt)
    {
        return new UserResponse(
            user.Id, user.UserName, user.Email, user.FirstName, user.LastName,
            user.RoleId, role, user.IsActive, user.MustChangePassword, createdAt);
    }
}
