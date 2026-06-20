namespace BG.Invoice.Domain.Exceptions;

public sealed class AccountLockedException : DomainException
{
    public AccountLockedException(string message) : base(message) { }
    public AccountLockedException(string message, Exception inner) : base(message, inner) { }
}
