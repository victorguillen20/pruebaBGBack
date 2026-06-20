namespace BG.Invoice.Application.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
