using System.Threading.RateLimiting;
using System.Globalization;
using Models.DTOs;

namespace WebExportAPI.Extensions;

public static class RateLimitingExtensions
{
    public const string LoginPolicy = "login";

    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
    {
        return services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            o.OnRejected = async (context, cancellationToken) =>
            {
                var response = context.HttpContext.Response;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);


                await response.WriteAsJsonAsync(new ErrorResponseDto
                {
                    ErrorCode = "RATE_LIMIT_EXCEEDED",
                    Message = "Zu viele Anfragen. Bitte versuchen Sie es später erneut."
                }, cancellationToken: cancellationToken);
            };

            o.AddPolicy(LoginPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
        });
    }
}