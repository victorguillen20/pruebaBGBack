using System.Linq.Expressions;
using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Services;
using BG.Invoice.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BG.Invoice.UnitTests.Services;

public class RoleServiceTests
{
    private readonly Mock<IRepository<Role>> _repository = new();
    private readonly ILogger<RoleService> _logger = Mock.Of<ILogger<RoleService>>();
    private readonly RoleService _sut;

    public RoleServiceTests()
    {
        _sut = new RoleService(_repository.Object, _logger);
    }

    [Fact]
    public async Task ListAsync_ReturnsActiveRoles()
    {
        var roles = new List<Role>
        {
            Role.Create("Admin", "Administrator"),
            Role.Create("Vendedor", "Sales person")
        }.AsReadOnly();

        _repository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        var result = await _sut.ListAsync();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Admin");
        result[1].Name.Should().Be("Vendedor");
    }
}
