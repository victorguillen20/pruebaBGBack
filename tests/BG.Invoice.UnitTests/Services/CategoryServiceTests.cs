using System.Linq.Expressions;
using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Common;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Services;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace BG.Invoice.UnitTests.Services;

public class CategoryServiceTests
{
    private readonly Mock<IRepository<Category>> _repository = new();
    private readonly Mock<IRepository<Product>> _productRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ILogger<CategoryService> _logger = Mock.Of<ILogger<CategoryService>>();
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _sut = new CategoryService(_repository.Object, _productRepository.Object, _unitOfWork.Object, _logger);
    }

    private static Category CreateCategory(int id, string name)
    {
        var category = Category.Create(name);
        category.Id = id;
        return category;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCategory_ReturnsCategoryResponse()
    {
        var category = CreateCategory(1, "Electronics");

        _repository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await _sut.GetByIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task CreateAsync_UniqueName_CreatesCategory()
    {
        var request = new CreateCategoryRequest("New Category");

        _repository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>().AsReadOnly());

        var result = await _sut.CreateAsync(request);

        result.IsSuccess.Should().BeTrue();
        _repository.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_WithActiveProducts_ThrowsBusinessRuleException()
    {
        var category = CreateCategory(1, "Electronics");

        _repository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        var product = Product.Create("P001", "Test", 10m, 1);
        _productRepository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product }.AsReadOnly());

        var act = async () => await _sut.DeactivateAsync(1);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*active product*");
    }
}
