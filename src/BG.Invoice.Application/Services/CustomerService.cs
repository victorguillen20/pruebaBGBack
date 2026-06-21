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

public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(IRepository<Customer> repository, IUnitOfWork unitOfWork, ILogger<CustomerService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CustomerResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var customer = await _repository.GetByIdAsync(id, ct);
        if (customer is null)
            throw new NotFoundException("Customer", id);
        var createdAt = DateTime.UtcNow;
        return Result.Success(customer.ToResponse(createdAt));
    }

    public async Task<PagedResult<CustomerResponse>> SearchAsync(string? search, int page, int pageSize, bool activeOnly = true, CancellationToken ct = default)
    {
        var all = await _repository.ListAsync(c =>
            (!activeOnly || c.IsActive) &&
            (string.IsNullOrWhiteSpace(search) ||
             c.Name.Contains(search) ||
             c.Identification.Contains(search)), ct);
        var total = all.Count;
        var items = all.Skip((page - 1) * pageSize).Take(pageSize).Select(c => c.ToResponse(DateTime.UtcNow)).ToList();
        return new PagedResult<CustomerResponse> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<Result<CustomerResponse>> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        var validator = new CreateCustomerValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return Result.ValidationError<CustomerResponse>(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var normalizedIdent = request.Identification.Replace("-", "").Trim();
        var normalizedPhone = request.Phone?.Replace("-", "").Trim();

        var existing = await _repository.ListAsync(c => c.Identification == normalizedIdent, ct);
        if (existing.Any())
            throw new BusinessRuleException(Errors.Customer.IdentificationExists);

        var customer = Customer.Create(normalizedIdent, request.Name, request.Type, normalizedPhone, request.Email, request.Address, request.CreditLimit);
        await _repository.AddAsync(customer, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        var createdAt = DateTime.UtcNow;
        return Result.Success(customer.ToResponse(createdAt));
    }

    public async Task<Result<CustomerResponse>> UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken ct = default)
    {
        var validator = new UpdateCustomerValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return Result.ValidationError<CustomerResponse>(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var customer = await _repository.GetByIdAsync(id, ct);
        if (customer is null)
            throw new NotFoundException("Customer", id);

        var normalizedPhone = request.Phone?.Replace("-", "").Trim();
        customer.Update(request.Name, request.Type, normalizedPhone, request.Email, request.Address, request.CreditLimit);
        _repository.Update(customer);
        await _unitOfWork.SaveChangesAsync(ct);
        var createdAt = DateTime.UtcNow;
        return Result.Success(customer.ToResponse(createdAt));
    }

    public async Task<Result> DeactivateAsync(int id, CancellationToken ct = default)
    {
        var customer = await _repository.GetByIdAsync(id, ct);
        if (customer is null)
            throw new NotFoundException("Customer", id);
        customer.Deactivate();
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ActivateAsync(int id, CancellationToken ct = default)
    {
        var customer = await _repository.GetByIdAsync(id, ct);
        if (customer is null)
            throw new NotFoundException("Customer", id);
        customer.Activate();
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
