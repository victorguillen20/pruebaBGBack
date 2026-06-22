using BG.Invoice.Api.Configuration;
using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Domain.Enums;
using BG.Invoice.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BG.Invoice.Api.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize(Policy = AuthorizationPolicies.Authenticated)]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ICurrentUser _currentUser;
    private readonly IInvoicePdfGenerator _pdfGenerator;

    public InvoicesController(IInvoiceService invoiceService, ICurrentUser currentUser, IInvoicePdfGenerator pdfGenerator)
    {
        _invoiceService = invoiceService;
        _currentUser = currentUser;
        _pdfGenerator = pdfGenerator;
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] int? customerId,
        [FromQuery] int? sellerId,
        [FromQuery] InvoiceStatus? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] decimal? minTotal,
        [FromQuery] decimal? maxTotal,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (pageSize > 100) pageSize = 100;
        var criteria = new InvoiceSearchCriteria(
            search, customerId, sellerId, status, fromDate, toDate,
            minTotal, maxTotal, page, pageSize);
        var result = await _invoiceService.SearchAsync(criteria, _currentUser.UserId, _currentUser.IsAdmin, ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _invoiceService.GetByIdAsync(id, _currentUser.UserId, _currentUser.IsAdmin, ct);
        return Ok(result.Value);
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> GetPdf(int id, CancellationToken ct)
    {
        var result = await _invoiceService.GetByIdAsync(id, _currentUser.UserId, _currentUser.IsAdmin, ct);
        if (result.Value is null)
            throw new NotFoundException("Invoice", id);
        var pdfBytes = await _pdfGenerator.GenerateAsync(result.Value, ct);
        return File(pdfBytes, "application/pdf", $"factura-{result.Value.Number}.pdf");
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.VendorOrAdmin)]
    public async Task<IActionResult> Create(CreateInvoiceRequest request, CancellationToken ct)
    {
        var result = await _invoiceService.CreateAsync(request, _currentUser.UserId, ct);
        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = string.Join("; ", result.ValidationErrors),
                Status = StatusCodes.Status400BadRequest
            });
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Policy = AuthorizationPolicies.VendorOrAdmin)]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        await _invoiceService.CancelAsync(id, _currentUser.UserId, ct);
        return NoContent();
    }

    [HttpPost("{id}/payments")]
    [Authorize(Policy = AuthorizationPolicies.VendorOrAdmin)]
    public async Task<IActionResult> AddPayment(int id, AddPaymentRequest request, CancellationToken ct)
    {
        var result = await _invoiceService.AddPaymentAsync(id, request, ct);
        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = string.Join("; ", result.ValidationErrors),
                Status = StatusCodes.Status400BadRequest
            });
        return NoContent();
    }
}
