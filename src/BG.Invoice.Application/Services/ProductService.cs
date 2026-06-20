using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Common;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Mappings;
using BG.Invoice.Application.Validators;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BG.Invoice.Application.Services;

public class ProductService : IProductService
{
    private readonly IRepository<Product> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IRepository<Product> repository, IUnitOfWork unitOfWork, ILogger<ProductService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProductResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(id, ct);
        if (product is null)
            throw new NotFoundException("Product", id);
        var createdAt = DateTime.UtcNow;
        return Result.Success(product.ToResponse(product.Category?.Name ?? "", createdAt));
    }

    public async Task<PagedResult<ProductResponse>> SearchAsync(string? search, int? categoryId, int page, int pageSize, CancellationToken ct = default)
    {
        var all = await _repository.ListAsync(p =>
            p.IsActive &&
            (!categoryId.HasValue || p.CategoryId == categoryId) &&
            (string.IsNullOrWhiteSpace(search) || p.Name.Contains(search) || p.Code.Contains(search)), ct);
        var total = all.Count;
        var items = all.Skip((page - 1) * pageSize).Take(pageSize).Select(p => p.ToResponse(p.Category?.Name ?? "", DateTime.UtcNow)).ToList();
        return new PagedResult<ProductResponse> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<Result<ProductResponse>> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        var validator = new CreateProductValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return Result.ValidationError<ProductResponse>(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var existing = await _repository.ListAsync(p => p.Code == request.Code, ct);
        if (existing.Any())
            throw new BusinessRuleException(Errors.Product.CodeExists);

        var product = Product.Create(request.Code, request.Name, request.Price, request.CategoryId, request.Stock, request.Cost, request.Description);
        await _repository.AddAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        var createdAt = DateTime.UtcNow;
        return Result.Success(product.ToResponse(product.Category?.Name ?? "", createdAt));
    }

    public async Task<Result<ProductResponse>> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var validator = new UpdateProductValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return Result.ValidationError<ProductResponse>(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var product = await _repository.GetByIdAsync(id, ct);
        if (product is null)
            throw new NotFoundException("Product", id);

        product.Update(request.Name, request.Price, request.CategoryId, request.Cost, request.Description);
        _repository.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);
        var createdAt = DateTime.UtcNow;
        return Result.Success(product.ToResponse(product.Category?.Name ?? "", createdAt));
    }

    public async Task<Result> DeactivateAsync(int id, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(id, ct);
        if (product is null)
            throw new NotFoundException("Product", id);
        product.Deactivate();
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ActivateAsync(int id, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(id, ct);
        if (product is null)
            throw new NotFoundException("Product", id);
        product.Activate();
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
