using Microsoft.AspNetCore.Mvc;
using Models.DTOs;
using Models.DTOs.Auth;
using WebExportAPI.Services;

namespace WebExportAPI.Endpoints;

public static class AuthEndPoint
{
    public static RouteHandlerBuilder MapAuthRoute(this IEndpointRouteBuilder routes)
    {
        return routes.MapPost("/api/v1/auth/token", async ([FromServices] IAuthService authService, [FromBody] LoginDto login, CancellationToken ct) =>
        {
            var res = await authService.LoginAsync(login, ct);

            return (res is null) ?
            Results.Json(new ErrorResponseDto
            {
                ErrorCode = "AUTHENTICATION_FAILED",
                Message = "Client-ID oder Client-Secret ist falsch.",
                Timestamp = DateTime.UtcNow
            },
            statusCode: StatusCodes.Status401Unauthorized)
            : Results.Ok(res);
        });
    }
}