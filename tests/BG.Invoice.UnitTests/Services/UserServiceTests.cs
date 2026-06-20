using System.Linq.Expressions;
using System.Reflection;
using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Common;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Services;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace BG.Invoice.UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repository = new();
    private readonly Mock<IRepository<Role>> _roleRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ILogger<UserService> _logger = Mock.Of<ILogger<UserService>>();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_repository.Object, _roleRepository.Object, _unitOfWork.Object, _logger);
    }

    private static User CreateUser(int id, string userName, int roleId, bool isActive = true)
    {
        var user = User.Create(userName, "test@test.com", "hash", "First", "Last", roleId);
        typeof(User).GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(user, id);
        typeof(User).GetField("<IsActive>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(user, isActive);
        return user;
    }

    private static Role CreateRole(int id, string name)
    {
        var role = Role.Create(name);
        typeof(Role).GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(role, id);
        return role;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsUserResponse()
    {
        var user = CreateUser(1, "admin", 1);
        var role = CreateRole(1, "Admin");
        typeof(User).GetField("<Role>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(user, role);

        _repository.Setup(r => r.GetByIdWithRoleAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetByIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(1);
        result.Value.UserName.Should().Be("admin");
        result.Value.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentUser_ThrowsNotFoundException()
    {
        _repository.Setup(r => r.GetByIdWithRoleAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = async () => await _sut.GetByIdAsync(99);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*User*");
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesUserFields()
    {
        var user = CreateUser(1, "admin", 1);
        var request = new UpdateUserRequest("NewFirst", "NewLast", 2, false);

        _repository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.UpdateAsync(1, request);

        result.IsSuccess.Should().BeTrue();
        user.FirstName.Should().Be("NewFirst");
        user.LastName.Should().Be("NewLast");
        user.RoleId.Should().Be(2);
        user.IsActive.Should().BeFalse();
        _repository.Verify(r => r.Update(user), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_LastActiveAdmin_ThrowsBusinessRuleException()
    {
        var adminRole = CreateRole(1, "Admin");
        var user = CreateUser(1, "admin", 1);

        _repository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _roleRepository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role> { adminRole }.AsReadOnly());
        _repository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>().AsReadOnly());

        var act = async () => await _sut.DeactivateAsync(1);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage(Errors.User.LastActiveAdmin);
    }
}
