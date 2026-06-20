using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Mappings;
using BG.Invoice.Application.Validators;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Enums;
using BG.Invoice.Domain.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BG.Invoice.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IRepository<Domain.Entities.Invoice> _invoiceRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IInvoiceNumberGenerator _numberGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        IRepository<Domain.Entities.Invoice> invoiceRepository,
        IRepository<Customer> customerRepository,
        IRepository<Product> productRepository,
        IInvoiceNumberGenerator numberGenerator,
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<InvoiceService> logger)
    {
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _numberGenerator = numberGenerator;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result<InvoiceResponse>> GetByIdAsync(int id, int requestingUserId, bool isAdmin, CancellationToken ct = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, ct);
        if (invoice is null)
            return Result.Failure<InvoiceResponse>($"Invoice with id '{id}' was not found.");

        if (!isAdmin && invoice.SellerId != requestingUserId)
            return Result.Failure<InvoiceResponse>("Access denied.");

        var createdAt = DateTime.UtcNow;
        return Result.Success(invoice.ToResponse(createdAt));
    }

    public async Task<PagedResult<InvoiceSummaryResponse>> SearchAsync(InvoiceSearchCriteria criteria, int requestingUserId, bool isAdmin, CancellationToken ct = default)
    {
        var all = await _invoiceRepository.ListAsync(i =>
            (!criteria.CustomerId.HasValue || i.CustomerId == criteria.CustomerId) &&
            (!isAdmin && criteria.SellerId.HasValue || i.SellerId == requestingUserId || isAdmin) &&
            (!criteria.Status.HasValue || i.Status == criteria.Status) &&
            (!criteria.FromDate.HasValue || i.Date >= criteria.FromDate) &&
            (!criteria.ToDate.HasValue || i.Date <= criteria.ToDate) &&
            (!criteria.MinTotal.HasValue || i.Total >= criteria.MinTotal) &&
            (!criteria.MaxTotal.HasValue || i.Total <= criteria.MaxTotal), ct);

        var total = all.Count;
        var items = all
            .OrderByDescending(i => i.Date)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(i => i.ToSummaryResponse())
            .ToList();

        return new PagedResult<InvoiceSummaryResponse>
        {
            Items = items, Total = total, Page = criteria.Page, PageSize = criteria.PageSize
        };
    }

    public async Task<Result<InvoiceResponse>> CreateAsync(CreateInvoiceRequest request, int userId, CancellationToken ct = default)
    {
        var validator = new CreateInvoiceValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return Result.ValidationError<InvoiceResponse>(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, ct);
        if (customer is null)
            return Result.Failure<InvoiceResponse>("Customer not found.");

        if (request.Type == InvoiceType.Credito && !request.DueDate.HasValue)
            return Result.Failure<InvoiceResponse>("Credit invoices require a due date.");

        var number = await _numberGenerator.GenerateNextAsync(ct);

        var nowUtc = _clock.UtcNow;
        var invoice = Domain.Entities.Invoice.Create(number, nowUtc, request.CustomerId, userId, request.Type, request.DueDate, request.Notes);

        foreach (var detailRequest in request.Details)
        {
            var product = await _productRepository.GetByIdAsync(detailRequest.ProductId, ct);
            if (product is null)
                return Result.Failure<InvoiceResponse>($"Product with id '{detailRequest.ProductId}' not found.");

            if (!product.IsActive)
                return Result.Failure<InvoiceResponse>($"Product '{product.Name}' is inactive.");

            if (product.Stock < detailRequest.Quantity)
                return Result.Failure<InvoiceResponse>($"Insufficient stock for product '{product.Code}'. Available: {product.Stock}, requested: {detailRequest.Quantity}.");

            product.DecrementStock(detailRequest.Quantity);
            invoice.AddDetail(detailRequest.ProductId, detailRequest.Quantity, detailRequest.UnitPrice, detailRequest.ProductName, detailRequest.ProductCode);
        }

        var configRate = 0.13m;
        var taxAmount = Math.Round(invoice.Subtotal * configRate, 2, MidpointRounding.AwayFromZero);
        invoice.SetTaxAmount(taxAmount, configRate * 100);

        await _invoiceRepository.AddAsync(invoice, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        var createdAt = DateTime.UtcNow;
        return Result.Success(invoice.ToResponse(createdAt));
    }

    public async Task<Result> CancelAsync(int id, int userId, CancellationToken ct = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, ct);
        if (invoice is null)
            return Result.Failure($"Invoice with id '{id}' was not found.");

        if (invoice.Status == InvoiceStatus.Anulada)
            return Result.Failure("Invoice is already cancelled.");

        invoice.Cancel();
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> AddPaymentAsync(int invoiceId, AddPaymentRequest request, CancellationToken ct = default)
    {
        var validator = new AddPaymentValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return Result.ValidationError(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, ct);
        if (invoice is null)
            return Result.Failure($"Invoice with id '{invoiceId}' was not found.");

        var paymentDate = request.PaymentDate ?? _clock.UtcNow;
        invoice.AddPayment(request.Method, request.Amount, request.Reference, paymentDate);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
