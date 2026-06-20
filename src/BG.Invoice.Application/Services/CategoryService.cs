using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Mappings;
using BG.Invoice.Application.Validators;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BG.Invoice.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IRepository<Category> repository, IUnitOfWork unitOfWork, ILogger<CategoryService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CategoryResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var category = await _repository.GetByIdAsync(id, ct);
        if (category is null)
            return Result.Failure<CategoryResponse>($"Category with id '{id}' was not found.");
        var createdAt = DateTime.UtcNow;
        return Result.Success(category.ToResponse(createdAt));
    }

    public async Task<PagedResult<CategoryResponse>> SearchAsync(string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var all = await _repository.ListAsync(c =>
            c.IsActive &&
            (string.IsNullOrWhiteSpace(search) || c.Name.Contains(search)), ct);
        var total = all.Count;
        var items = all.Skip((page - 1) * pageSize).Take(pageSize).Select(c => c.ToResponse(DateTime.UtcNow)).ToList();
        return new PagedResult<CategoryResponse> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<Result<CategoryResponse>> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        var validator = new CreateCategoryValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return Result.ValidationError<CategoryResponse>(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var existing = await _repository.ListAsync(c => c.Name == request.Name, ct);
        if (existing.Any())
            return Result.Failure<CategoryResponse>("Category name already exists.");

        var category = Category.Create(request.Name);
        await _repository.AddAsync(category, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        var createdAt = DateTime.UtcNow;
        return Result.Success(category.ToResponse(createdAt));
    }

    public async Task<Result<CategoryResponse>> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var validator = new UpdateCategoryValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return Result.ValidationError<CategoryResponse>(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var category = await _repository.GetByIdAsync(id, ct);
        if (category is null)
            return Result.Failure<CategoryResponse>($"Category with id '{id}' was not found.");

        category.Update(request.Name);
        _repository.Update(category);
        await _unitOfWork.SaveChangesAsync(ct);
        var createdAt = DateTime.UtcNow;
        return Result.Success(category.ToResponse(createdAt));
    }

    public async Task<Result> DeactivateAsync(int id, CancellationToken ct = default)
    {
        var category = await _repository.GetByIdAsync(id, ct);
        if (category is null)
            return Result.Failure($"Category with id '{id}' was not found.");
        category.Deactivate();
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
