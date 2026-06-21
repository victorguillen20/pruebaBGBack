using System.Linq.Expressions;
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
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ILogger<UserService> _logger = Mock.Of<ILogger<UserService>>();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _passwordHasher.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed_password");
        _sut = new UserService(_repository.Object, _roleRepository.Object, _passwordHasher.Object, _unitOfWork.Object, _logger);
    }

    private static User CreateUser(int id, string userName, int roleId, bool isActive = true)
    {
        var user = User.Create(userName, "test@test.com", "hash", "First", "Last", roleId);
        user.Id = id;
        user.IsActive = isActive;
        return user;
    }

    private static Role CreateRole(int id, string name)
    {
        var role = Role.Create(name);
        role.Id = id;
        return role;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsUserResponse()
    {
        var user = CreateUser(1, "admin", 1);
        var role = CreateRole(1, "Admin");
        user.Role = role;

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
        var request = new UpdateUserRequest("NewFirst", "NewLast", 2);

        _repository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.UpdateAsync(1, request);

        result.IsSuccess.Should().BeTrue();
        user.FirstName.Should().Be("NewFirst");
        user.LastName.Should().Be("NewLast");
        user.RoleId.Should().Be(2);
        _repository.Verify(r => r.Update(user), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotChangeIsActive()
    {
        var user = CreateUser(1, "admin", 1);
        var request = new UpdateUserRequest("NewFirst", "NewLast", 2);

        _repository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.UpdateAsync(1, request);

        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_DoesNotReactivateDeactivatedUser()
    {
        var user = CreateUser(1, "admin", 1, isActive: false);
        var request = new UpdateUserRequest("NewFirst", "NewLast", 2);

        _repository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.UpdateAsync(1, request);

        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeFalse();
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

    [Fact]
    public async Task CreateAsync_ValidRequest_HashesPassword()
    {
        var request = new CreateUserRequest("newuser", "new@test.com", "P@ssword123", "P@ssword123", "New", "User", 2);

        _repository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>().AsReadOnly());
        _repository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.CreateAsync(request);

        result.IsSuccess.Should().BeTrue();
        _passwordHasher.Verify(p => p.Hash("P@ssword123"), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateUsername_ReturnsFailure()
    {
        var request = new CreateUserRequest("existing", "new@test.com", "P@ssword123", "P@ssword123", "New", "User", 2);

        _repository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { CreateUser(99, "existing", 2) }.AsReadOnly());

        var result = await _sut.CreateAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be(Errors.User.UsernameTaken);
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ReturnsFailure()
    {
        var request = new CreateUserRequest("newuser", "used@test.com", "P@ssword123", "P@ssword123", "New", "User", 2);

        _repository.SetupSequence(r => r.ListAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>().AsReadOnly()) // username check passes
            .ReturnsAsync(new List<User> { CreateUser(99, "someone", 2) }.AsReadOnly()); // email check fails

        var result = await _sut.CreateAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be(Errors.User.EmailRegistered);
    }
}
