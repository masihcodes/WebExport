using System.Security.Claims;
using Models.DTOs;
using Models.DTOs.Dashboard;
using WebExportAPI.src.Services;

namespace WebExportAPI.src.Endpoints;

public static class DashboardEndPoint
{
    public static void MapDashboardRoute(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/dashboard").RequireAuthorization();

        group.MapGet("summary", async (IDashboardService dashboardService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            if (!short.TryParse(user.FindFirstValue("tenant"), out var tenantNumber))
            {
                return Results.Json(new ErrorResponseDto
                {
                    ErrorCode = "UNAUTHORIZED",
                    Message = "Das Token enthaltet keine gültige Mandantennummer."
                },
                statusCode: StatusCodes.Status401Unauthorized);
            }

            DashboardSummaryDto res = await dashboardService.GetSummaryAsync(tenantNumber, ct);

            return Results.Ok(res);
        });


        group.MapGet("stock", async (IDashboardService dashboardService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            if (!short.TryParse(user.FindFirstValue("tenant"), out var tenantNumber))
            {
                return Results.Json(new ErrorResponseDto
                {
                    ErrorCode = "UNAUTHORIZED",
                    Message = "Das Token enthaltet keine gültige Mandantennummer."
                },
                statusCode: StatusCodes.Status401Unauthorized);
            }

            var res = await dashboardService.GetStockByBranchAsync(tenantNumber, ct);

            return Results.Ok(res);
        });

        group.MapGet("recent", async (IDashboardService dashboardService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            if (!short.TryParse(user.FindFirstValue("tenant"), out var tenantNumber))
            {
                return Results.Json(new ErrorResponseDto
                {
                    ErrorCode = "UNAUTHORIZED",
                    Message = "Das Token enthaltet keine gültige Mandantennummer."
                },
                statusCode: StatusCodes.Status401Unauthorized);
            }

            var res = await dashboardService.GetRecentArticlesAsync(tenantNumber, ct);

            return Results.Ok(res);
        });

        group.MapGet("top-categories", async (IDashboardService dashboardService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            if (!short.TryParse(user.FindFirstValue("tenant"), out var tenantNumber))
            {
                return Results.Json(new ErrorResponseDto
                {
                    ErrorCode = "UNAUTHORIZED",
                    Message = "Das Token enthaltet keine gültige Mandantennummer."
                },
                statusCode: StatusCodes.Status401Unauthorized);
            }

            var res = await dashboardService.GetTopCategoriesAsync(tenantNumber, ct);

            return Results.Ok(res);
        });

        group.MapGet("quality", async (IDashboardService dashboardService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            if (!short.TryParse(user.FindFirstValue("tenant"), out var tenantNumber))
            {
                return Results.Json(new ErrorResponseDto
                {
                    ErrorCode = "UNAUTHORIZED",
                    Message = "Das Token enthaltet keine gültige Mandantennummer."
                },
                statusCode: StatusCodes.Status401Unauthorized);
            }

            var res = await dashboardService.GetContentQualityAsync(tenantNumber, ct);

            return Results.Ok(res);
        });
    }
}
