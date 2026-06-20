using BG.Invoice.Api.Configuration;
using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BG.Invoice.Api.Controllers;

[ApiController]
[Route("api/company-config")]
[Authorize(Policy = AuthorizationPolicies.Authenticated)]
public class CompanyConfigController : ControllerBase
{
    private readonly ICompanyConfigService _companyConfigService;

    public CompanyConfigController(ICompanyConfigService companyConfigService)
    {
        _companyConfigService = companyConfigService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await _companyConfigService.GetAsync(ct);
        return Ok(result.Value);
    }

    [HttpPut]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> Update(UpdateCompanyConfigRequest request, CancellationToken ct)
    {
        var result = await _companyConfigService.UpdateAsync(request, ct);
        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = string.Join("; ", result.ValidationErrors),
                Status = StatusCodes.Status400BadRequest
            });
        return Ok(result.Value);
    }
}
