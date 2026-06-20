using BG.Invoice.Application.Dtos;

namespace BG.Invoice.Application.Abstractions;

public interface ICompanyConfigService
{
    Task<Result<CompanyConfigResponse>> GetAsync(CancellationToken ct = default);
    Task<Result<CompanyConfigResponse>> UpdateAsync(UpdateCompanyConfigRequest request, CancellationToken ct = default);
}
