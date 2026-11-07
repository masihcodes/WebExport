using Models.DTOs;

namespace WebExportAPI.Middleware;

public class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var apiKey = configuration["ApiKey"];
        // var apiKey = configuration.GetValue<string>("ApiKey");
        if ((context.Request.Path.Value ?? "").Contains("/api"))
        {
            if (!context.Request.Headers.TryGetValue("X-API-KEY", out var extractedApiKey) || apiKey != extractedApiKey)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new ErrorResponseDto
                {
                    ErrorCode = "UNAUTHORIZED",
                    Message = "Der API-Schlüssel fehlt oder ist ungültig."
                });
                return;
            }
        }

        await next(context);
    }

    // public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    // {
    //     if (context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
    //     {
    //         if (!context.Request.ContentType?.Contains("application/json") ?? true)
    //         {
    //             context.Response.StatusCode = StatusCodes.Status400BadRequest;
    //             await context.Response.WriteAsync("Invalid content type");
    //             return;
    //         }
    //         var apiKey = configuration["ApiKey"];

    //         context.Request.EnableBuffering();
    //         using var document = await JsonDocument.ParseAsync(context.Request.Body, cancellationToken: context.RequestAborted);
    //         var body = document.RootElement;
    //         context.Request.Body.Position = 0;

    //         if (!body.TryGetProperty("ApiKey", out var extractedApiKey) || apiKey != extractedApiKey.GetString())
    //         {
    //             context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    //             await context.Response.WriteAsync("API Key is missing.");
    //             return;
    //         }
    //     }
    //     await next(context);
    // }
}

