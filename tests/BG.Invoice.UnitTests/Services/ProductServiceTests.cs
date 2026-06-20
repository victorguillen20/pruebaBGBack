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

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ILogger<ProductService> _logger = Mock.Of<ILogger<ProductService>>();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _sut = new ProductService(_repository.Object, _unitOfWork.Object, _logger);
    }

    private static Product CreateProduct(int id, string code)
    {
        var product = Product.Create(code, "Test Product", 22.00m, 1, 100);
        typeof(Product).GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(product, id);
        return product;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingProduct_ReturnsProductResponse()
    {
        var product = CreateProduct(1, "P001");
        var category = Category.Create("Electronics");
        typeof(Category).GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(category, 1);
        typeof(Product).GetField("<Category>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(product, category);

        _repository.Setup(r => r.GetByIdWithCategoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _sut.GetByIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Code.Should().Be("P001");
        result.Value.CategoryName.Should().Be("Electronics");
    }

    [Fact]
    public async Task CreateAsync_UniqueCode_CreatesProduct()
    {
        var request = new CreateProductRequest("P001", "New Product", 22.00m, 1, 100, 10m, null);

        _repository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>().AsReadOnly());

        var result = await _sut.CreateAsync(request);

        result.IsSuccess.Should().BeTrue();
        _repository.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_ThrowsBusinessRuleException()
    {
        var existing = CreateProduct(1, "P001");
        var request = new CreateProductRequest("P001", "New Product", 22.00m, 1, 100, 10m, null);

        _repository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { existing }.AsReadOnly());

        var act = async () => await _sut.CreateAsync(request);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage(Errors.Product.CodeExists);
    }
}
