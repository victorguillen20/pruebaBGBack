namespace BG.Invoice.Api.Middleware;

public class RequestIdMiddleware
{
    private readonly RequestDelegate _next;

    public RequestIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.Request.Headers.TryGetValue("X-Request-Id", out var headerValue)
            ? headerValue.ToString()
            : Guid.NewGuid().ToString("N");

        context.TraceIdentifier = requestId;
        context.Items["RequestId"] = requestId;
        context.Response.Headers["X-Request-Id"] = requestId;

        await _next(context);
    }
}
