namespace BG.Invoice.Application.Dtos;

public record MenuResponse(
    int Id,
    string Key,
    string Label,
    string Icon,
    string Route,
    int Order
);
