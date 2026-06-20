namespace BG.Invoice.Application.Common;

public class Result
{
    public bool IsSuccess { get; protected set; }
    public bool IsFailure => !IsSuccess;
    public List<string> ValidationErrors { get; protected set; } = new();
    public string? ErrorMessage { get; protected set; }

    public static Result Success() => new() { IsSuccess = true };
    public static Result Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
    public static Result ValidationError(List<string> errors) => new() { IsSuccess = false, ValidationErrors = errors };
    public static Result ValidationError(string error) => new() { IsSuccess = false, ValidationErrors = new() { error } };
    public static Result<T> Success<T>(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure<T>(string error) => new() { IsSuccess = false, ErrorMessage = error };
    public static Result<T> ValidationError<T>(List<string> errors) => new() { IsSuccess = false, ValidationErrors = errors };
    public static Result<T> ValidationError<T>(string error) => new() { IsSuccess = false, ValidationErrors = new() { error } };
}

public class Result<T> : Result
{
    public T? Value { get; set; }
}
