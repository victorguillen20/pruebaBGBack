namespace BG.Invoice.Application.Common;

public class ServiceError
{
    public string Code { get; set; } = default!;
    public string Message { get; set; } = default!;
    public int? HttpStatus { get; set; }

    public ServiceError() { }

    public ServiceError(string code, string message, int? httpStatus = null)
    {
        Code = code;
        Message = message;
        HttpStatus = httpStatus;
    }

    public static ServiceError NotFound(string entity, object key) =>
        new("NOT_FOUND", $"{entity} with key '{key}' was not found.", 404);

    public static ServiceError Conflict(string message) =>
        new("CONFLICT", message, 409);

    public static ServiceError Unauthorized(string message = "Unauthorized.") =>
        new("UNAUTHORIZED", message, 401);

    public static ServiceError Forbidden(string message = "Forbidden.") =>
        new("FORBIDDEN", message, 403);

    public static ServiceError BadRequest(string message) =>
        new("BAD_REQUEST", message, 400);

    public static ServiceError Validation(string message) =>
        new("VALIDATION_ERROR", message, 400);
}
