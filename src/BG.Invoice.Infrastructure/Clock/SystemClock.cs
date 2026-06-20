using BG.Invoice.Application.Abstractions;

namespace BG.Invoice.Infrastructure.Clock;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
