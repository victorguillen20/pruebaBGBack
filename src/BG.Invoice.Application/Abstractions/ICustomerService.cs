using BG.Invoice.Application.Dtos;

namespace BG.Invoice.Application.Abstractions;

public interface ICustomerService
{
    Task<Result<CustomerResponse>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<CustomerResponse>> SearchAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<Result<CustomerResponse>> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default);
    Task<Result<CustomerResponse>> UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken ct = default);
    Task<Result> DeactivateAsync(int id, CancellationToken ct = default);
    Task<Result> ActivateAsync(int id, CancellationToken ct = default);
}
