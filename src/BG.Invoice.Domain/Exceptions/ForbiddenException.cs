namespace BG.Invoice.Domain.Exceptions;

public sealed class ForbiddenException : DomainException
{
    public ForbiddenException(string message) : base(message) { }
    public ForbiddenException(string message, Exception inner) : base(message, inner) { }
}
