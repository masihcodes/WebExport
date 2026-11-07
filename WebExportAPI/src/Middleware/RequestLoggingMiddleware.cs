using System.Diagnostics;

namespace WebExportAPI.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await next(context);
            return;
        }
        var requestId = context.TraceIdentifier;
        using IDisposable? scope = logger.BeginScope("RequestId: {RequestId}", requestId);
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Request-ID"] = requestId;
            return Task.CompletedTask;
        });

        var stopwatch = Stopwatch.StartNew();
        int? statusCode = null;
        try
        {
            await next(context);
            statusCode = context.Response.StatusCode;
        }
        finally
        {
            stopwatch.Stop();
            logger.LogInformation(
                "HTTP Completed: {Method} ➤ {Path} ➤ Code {StatusCode} in {ElapsedMs} ms ➤ RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                statusCode,
                stopwatch.Elapsed.TotalMilliseconds,
                requestId
            );
        }
    }
}
