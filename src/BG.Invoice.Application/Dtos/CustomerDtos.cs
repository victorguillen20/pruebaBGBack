using BG.Invoice.Domain.Enums;

namespace BG.Invoice.Application.Dtos;

public record CustomerResponse(
    int Id,
    string Identification,
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    CustomerType Type,
    bool IsActive,
    decimal? CreditLimit,
    DateTime CreatedAt
);

public record CreateCustomerRequest(
    string Identification,
    string Name,
    CustomerType Type,
    string? Phone,
    string? Email,
    string? Address,
    decimal? CreditLimit
);

public record UpdateCustomerRequest(
    string Name,
    CustomerType Type,
    string? Phone,
    string? Email,
    string? Address,
    decimal? CreditLimit
);
