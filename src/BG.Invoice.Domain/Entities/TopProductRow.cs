namespace BG.Invoice.Domain.Entities;

public record TopProductRow(
    int ProductId,
    string ProductCode,
    string ProductName,
    int TotalQuantitySold,
    decimal TotalRevenue
);
