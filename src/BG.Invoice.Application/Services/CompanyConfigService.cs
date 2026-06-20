using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Mappings;
using BG.Invoice.Application.Validators;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BG.Invoice.Application.Services;

public class CompanyConfigService : ICompanyConfigService
{
    private readonly IRepository<CompanyConfig> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompanyConfigService> _logger;

    public CompanyConfigService(IRepository<CompanyConfig> repository, IUnitOfWork unitOfWork, ILogger<CompanyConfigService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CompanyConfigResponse>> GetAsync(CancellationToken ct = default)
    {
        var all = await _repository.ListAsync(ct: ct);
        var config = all.FirstOrDefault();
        if (config is null)
            throw new NotFoundException("CompanyConfig", "singleton");
        return Result.Success(config.ToResponse());
    }

    public async Task<Result<CompanyConfigResponse>> UpdateAsync(UpdateCompanyConfigRequest request, CancellationToken ct = default)
    {
        var validator = new UpdateCompanyConfigValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return Result.ValidationError<CompanyConfigResponse>(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var all = await _repository.ListAsync(ct: ct);
        var config = all.FirstOrDefault();
        if (config is null)
            throw new NotFoundException("CompanyConfig", "singleton");

        config.Update(request.CompanyName, request.TaxPercent, request.CurrencySymbol,
            request.Phone, request.Email, request.TaxId,
            request.Address, request.City, request.Region,
            request.PostalCode, request.LogoUrl);
        _repository.Update(config);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success(config.ToResponse());
    }
}
