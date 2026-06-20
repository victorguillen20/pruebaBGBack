using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Common;
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
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IInvoiceNumberGenerator _numberGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        IInvoiceRepository invoiceRepository,
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
        var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(id, ct);
        if (invoice is null)
            throw new NotFoundException("Invoice", id);

        if (!isAdmin && invoice.SellerId != requestingUserId)
            throw new ForbiddenException(Errors.Invoice.AccessDenied);

        var createdAt = DateTime.UtcNow;
        return Result.Success(invoice.ToResponse(createdAt));
    }

    public async Task<PagedResult<InvoiceSummaryResponse>> SearchAsync(InvoiceSearchCriteria criteria, int requestingUserId, bool isAdmin, CancellationToken ct = default)
    {
        var effectiveSellerId = isAdmin ? criteria.SellerId : requestingUserId;

        var (items, total) = await _invoiceRepository.SearchPagedAsync(
            criteria.Search, criteria.CustomerId, effectiveSellerId, criteria.Status,
            criteria.FromDate, criteria.ToDate, criteria.MinTotal, criteria.MaxTotal,
            criteria.Page, criteria.PageSize, ct);

        var summaries = items.Select(i => i.ToSummaryResponse()).ToList();
        return new PagedResult<InvoiceSummaryResponse>
        {
            Items = summaries, Total = total, Page = criteria.Page, PageSize = criteria.PageSize
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
            throw new NotFoundException("Customer", request.CustomerId);

        if (request.Type == InvoiceType.Credito && !request.DueDate.HasValue)
            throw new BusinessRuleException(Errors.Invoice.CreditRequiresDueDate);

        var number = await _numberGenerator.GenerateNextAsync(ct);

        var nowUtc = _clock.UtcNow;
        var invoice = Domain.Entities.Invoice.Create(number, nowUtc, request.CustomerId, userId, request.Type, request.DueDate, request.Notes);

        foreach (var detailRequest in request.Details)
        {
            var product = await _productRepository.GetByIdAsync(detailRequest.ProductId, ct);
            if (product is null)
                throw new NotFoundException("Product", detailRequest.ProductId);

            if (!product.IsActive)
                throw new BusinessRuleException(string.Format(Errors.Invoice.ProductInactive, product.Name));

            if (product.Stock < detailRequest.Quantity)
                throw new BusinessRuleException(Errors.Invoice.InsufficientStock(product.Code, product.Stock, detailRequest.Quantity));

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
            throw new NotFoundException("Invoice", id);

        if (invoice.Status == InvoiceStatus.Anulada)
            throw new BusinessRuleException(Errors.Invoice.AlreadyCancelled);

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
            throw new NotFoundException("Invoice", invoiceId);

        var paymentDate = request.PaymentDate ?? _clock.UtcNow;
        invoice.AddPayment(request.Method, request.Amount, request.Reference, paymentDate);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
