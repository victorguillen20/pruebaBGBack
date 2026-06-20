using System.Reflection;
using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Common;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Services;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace BG.Invoice.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ILogger<AuthService> _logger = Mock.Of<ILogger<AuthService>>();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _userRepository.Object,
            _passwordHasher.Object,
            _jwtTokenService.Object,
            _unitOfWork.Object,
            _logger);
    }

    private static User CreateUserWithRole(int id, string userName, string roleName)
    {
        var user = User.Create(userName, "test@test.com", "hash", "First", "Last", 1);
        typeof(User).GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(user, id);
        var role = Role.Create(roleName);
        typeof(Role).GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(role, 1);
        typeof(User).GetField("<Role>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(user, role);
        return user;
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsLoginResponse()
    {
        var user = CreateUserWithRole(1, "admin", "Admin");
        var request = new LoginRequest("admin", "Admin123!");

        _userRepository.Setup(r => r.GetByUserNameWithRoleAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(p => p.Verify("Admin123!", "hash")).Returns(true);
        _jwtTokenService.Setup(j => j.GenerateToken(1, "admin", "Admin", 1)).Returns("jwt-token");

        var result = await _sut.LoginAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("jwt-token");
        result.Value.User.UserName.Should().Be("admin");
        result.Value.User.Role.Should().Be("Admin");
        _userRepository.Verify(r => r.GetByUserNameWithRoleAsync("admin", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedException()
    {
        var user = User.Create("admin", "test@test.com", "hash", "First", "Last", 1);

        _userRepository.Setup(r => r.GetByUserNameWithRoleAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(p => p.Verify("wrong", "hash")).Returns(false);

        var act = async () => await _sut.LoginAsync(new LoginRequest("admin", "wrong"));

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage(Errors.Auth.InvalidCredentials);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_LockedAccount_ThrowsAccountLockedException()
    {
        var user = CreateUserWithRole(1, "admin", "Admin");
        typeof(User).GetField("<LockoutUntil>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(user, DateTime.UtcNow.AddMinutes(15));

        _userRepository.Setup(r => r.GetByUserNameWithRoleAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var act = async () => await _sut.LoginAsync(new LoginRequest("admin", "Admin123!"));

        await act.Should().ThrowAsync<AccountLockedException>()
            .WithMessage(Errors.Auth.AccountLocked);
    }

    [Fact]
    public async Task LoginAsync_InactiveAccount_ThrowsForbiddenException()
    {
        var user = CreateUserWithRole(1, "inactive", "Vendedor");
        typeof(User).GetField("<IsActive>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(user, false);

        _userRepository.Setup(r => r.GetByUserNameWithRoleAsync("inactive", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var act = async () => await _sut.LoginAsync(new LoginRequest("inactive", "Admin123!"));

        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage(Errors.Auth.AccountInactive);
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidRequest_ChangesPassword()
    {
        var user = CreateUserWithRole(1, "admin", "Admin");
        typeof(User).GetField("<PasswordHash>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(user, "oldhash");

        _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(p => p.Verify("current", "oldhash")).Returns(true);
        _passwordHasher.Setup(p => p.Hash("newpass12")).Returns("newhash");

        var result = await _sut.ChangePasswordAsync(
            new ChangePasswordRequest("current", "newpass12", "newpass12"), 1);

        result.IsSuccess.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
