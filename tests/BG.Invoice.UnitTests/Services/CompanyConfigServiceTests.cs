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

public class CompanyConfigServiceTests
{
    private readonly Mock<IRepository<CompanyConfig>> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ILogger<CompanyConfigService> _logger = Mock.Of<ILogger<CompanyConfigService>>();
    private readonly CompanyConfigService _sut;

    public CompanyConfigServiceTests()
    {
        _sut = new CompanyConfigService(_repository.Object, _unitOfWork.Object, _logger);
    }

    private static CompanyConfig CreateConfig()
    {
        return CompanyConfig.Create("Test Company", 13m, "$");
    }

    [Fact]
    public async Task GetAsync_ExistingConfig_ReturnsConfig()
    {
        var config = CreateConfig();

        _repository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<CompanyConfig, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CompanyConfig> { config }.AsReadOnly());

        var result = await _sut.GetAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value!.CompanyName.Should().Be("Test Company");
        result.Value.TaxPercent.Should().Be(13m);
    }

    [Fact]
    public async Task GetAsync_NoConfig_ThrowsNotFoundException()
    {
        _repository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<CompanyConfig, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CompanyConfig>().AsReadOnly());

        var act = async () => await _sut.GetAsync();

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesConfig()
    {
        var config = CreateConfig();

        _repository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<CompanyConfig, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CompanyConfig> { config }.AsReadOnly());

        var request = new UpdateCompanyConfigRequest(
            "Updated Company", 15m, "€", "555-0100", "info@test.com",
            "TX-123", "123 Street", "City", "Region", "12345", null);

        var result = await _sut.UpdateAsync(request);

        result.IsSuccess.Should().BeTrue();
        config.CompanyName.Should().Be("Updated Company");
        config.TaxPercent.Should().Be(15m);
        config.CurrencySymbol.Should().Be("€");
        _repository.Verify(r => r.Update(config), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NoConfig_ThrowsNotFoundException()
    {
        var request = new UpdateCompanyConfigRequest(
            "Updated", 15m, "$", null, null, null, null, null, null, null, null);

        _repository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<CompanyConfig, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CompanyConfig>().AsReadOnly());

        var act = async () => await _sut.UpdateAsync(request);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
