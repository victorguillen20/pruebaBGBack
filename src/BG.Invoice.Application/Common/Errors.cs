namespace BG.Invoice.Application.Common;

public static class Errors
{
    public static class Auth
    {
        public const string InvalidCredentials = "Invalid credentials.";
        public const string AccountInactive = "Account is inactive.";
        public const string AccountLocked = "Account is locked. Try again later.";
        public const string CurrentPasswordIncorrect = "Current password is incorrect.";
    }

    public static class User
    {
        public const string UsernameTaken = "Username is already taken.";
        public const string EmailRegistered = "Email is already registered.";
        public const string LastActiveAdmin = "Cannot delete the last active admin.";
    }

    public static class Customer
    {
        public const string IdentificationExists = "Identification already exists.";
    }

    public static class Category
    {
        public const string NameExists = "Category name already exists.";
        public static string HasActiveProducts(int count) =>
            $"Cannot delete category with {count} active product(s).";
    }

    public static class Product
    {
        public const string CodeExists = "Product code already exists.";
    }

    public static class Invoice
    {
        public const string AccessDenied = "Access denied.";
        public const string CreditRequiresDueDate = "Credit invoices require a due date.";
        public const string ProductInactive = "Product '{0}' is inactive.";
        public const string AlreadyCancelled = "Invoice is already cancelled.";
        public static string InsufficientStock(string code, int available, int requested) =>
            $"Insufficient stock for product '{code}'. Available: {available}, requested: {requested}.";
    }

    public static class Validation
    {
        public const string PasswordsDoNotMatch = "Passwords do not match.";
        public const string InvoiceDetailsRequired = "Invoice must have at least one detail.";
    }
}
