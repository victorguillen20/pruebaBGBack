using BG.Invoice.Domain.Common;
using BG.Invoice.Domain.Enums;
using BG.Invoice.Domain.Exceptions;

namespace BG.Invoice.Domain.Entities;

public class User : Entity
{
    public int Id { get; private set; }
    public string UserName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public int RoleId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool MustChangePassword { get; private set; } = true;
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutUntil { get; private set; }

    public Role Role { get; private set; } = default!;
    public ICollection<Invoice> InvoicesAsSeller { get; private set; } = new List<Invoice>();

    private User() { }

    public static User Create(string userName, string email, string passwordHash, string firstName, string lastName, int roleId)
    {
        if (string.IsNullOrWhiteSpace(userName)) throw new BusinessRuleException("UserName is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new BusinessRuleException("Email is required.");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new BusinessRuleException("PasswordHash is required.");
        if (string.IsNullOrWhiteSpace(firstName)) throw new BusinessRuleException("FirstName is required.");
        if (string.IsNullOrWhiteSpace(lastName)) throw new BusinessRuleException("LastName is required.");
        if (roleId <= 0) throw new BusinessRuleException("RoleId is required.");

        return new User
        {
            UserName = userName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            RoleId = roleId,
            IsActive = true,
            MustChangePassword = true,
            FailedLoginAttempts = 0
        };
    }

    public void RecordFailedLogin(int lockoutThreshold, TimeSpan lockoutDuration)
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= lockoutThreshold)
        {
            LockoutUntil = DateTime.UtcNow.Add(lockoutDuration);
        }
    }

    public void ResetFailedLogins()
    {
        FailedLoginAttempts = 0;
        LockoutUntil = null;
    }

    public bool IsLockedOut(DateTime nowUtc) =>
        LockoutUntil.HasValue && LockoutUntil.Value > nowUtc;

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        MustChangePassword = false;
        ResetFailedLogins();
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public void UpdateProfile(string firstName, string lastName, int roleId, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new BusinessRuleException("FirstName is required.");
        if (string.IsNullOrWhiteSpace(lastName)) throw new BusinessRuleException("LastName is required.");
        if (roleId <= 0) throw new BusinessRuleException("RoleId is required.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        RoleId = roleId;
        IsActive = isActive;
    }

    public string FullName => $"{FirstName} {LastName}";
}