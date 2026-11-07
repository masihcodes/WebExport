using System.Security.Claims;
using Models.DTOs;
using Models.DTOs.BranchStockResponse;
using Models.DTOs.PricesStockResponse;
using WebExportAPI.Services;

namespace WebExportAPI.Endpoints;

public static class StockEndpoint
{
    public static void MapStockRoutes(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/").RequireAuthorization();

        group.MapGet("prices-stock", async (
                                                        IStockService stockService,
                                                        ClaimsPrincipal user,
                                                        CancellationToken ct,
                                                        int page = 1,
                                                        int pageSize = 1000,
                                                        string? articleNumber = null,
                                                        short? branch = null,
                                                        DateTimeOffset? modifiedSince = null,
                                                        bool includeDeleted = false) =>
        {

            if (!short.TryParse(user.FindFirst("tenant")?.Value, out var tenantNumber))
            {
                return Results.Json(
                    new ErrorResponseDto
                    {
                        ErrorCode = "UNAUTHORIZED",
                        Message = "Das Token enthält keine gültige Mandantennummer."
                    },
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            if (page < 1 || pageSize < 1 || pageSize > 5000)
            {
                return Results.BadRequest(new ErrorResponseDto
                {
                    ErrorCode = "INVALID_PARAMETER",
                    Message = "page muss mindestens 1 sein. pageSize muss zwischen 1 und 5000 liegen.",
                    Timestamp = DateTime.UtcNow
                });
            }

            var query = new PricesStockListQueryDto
            {
                Page = page,
                PageSize = pageSize,
                ArticleNumber = articleNumber,
                Branch = branch,
                ModifiedSince = modifiedSince,
                IncludeDeleted = includeDeleted,
            };

            if (branch is short branchNumber && !await stockService.BranchExistsAsync(tenantNumber, branchNumber, ct))
            {
                return Results.BadRequest(new ErrorResponseDto
                {
                    ErrorCode = "INVALID_PARAMETER",
                    Message = "Die angegebene Filiale wurde nicht gefunden.",
                    Timestamp = DateTime.UtcNow
                });
            }

            PagedResponseDto<PricesStockResponseDto> res = await stockService.GetPricesStockAsync(tenantNumber, query, ct);

            return Results.Ok(res);

        });



        group.MapGet("branch-stock", async (
                                                        IStockService stockService,
                                                        ClaimsPrincipal user,
                                                        CancellationToken ct,
                                                        int page = 1,
                                                        int pageSize = 1000,
                                                        string? sku = null,
                                                        string? articleNumber = null,
                                                        short? branch = null,
                                                        DateTimeOffset? modifiedSince = null) =>
        {

            if (!short.TryParse(user.FindFirst("tenant")?.Value, out var tenantNumber))
            {
                return Results.Json(new ErrorResponseDto
                {
                    ErrorCode = "UNAUTHORIZED",
                    Message = "Das Token enthält keine gültige Mandantennummer."
                },
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            if (page < 1 || pageSize < 1 || pageSize > 5000)
            {
                return Results.BadRequest(new ErrorResponseDto
                {
                    ErrorCode = "INVALID_PARAMETER",
                    Message = "page muss mindestens 1 sein. pageSize muss zwischen 1 und 5000 liegen.",
                    Timestamp = DateTime.UtcNow
                });
            }

            var query = new BranchStockListQueryDto
            {
                Page = page,
                PageSize = pageSize,
                ArticleNumber = articleNumber,
                Branch = branch,
                ModifiedSince = modifiedSince,
                Sku = sku,
            };

            if (branch is short branchNumber && !await stockService.BranchExistsAsync(tenantNumber, branchNumber, ct))
            {
                return Results.BadRequest(new ErrorResponseDto
                {
                    ErrorCode = "INVALID_PARAMETER",
                    Message = "Die angegebene Filiale wurde nicht gefunden.",
                    Timestamp = DateTime.UtcNow
                });
            }

            PagedResponseDto<BranchStockResponseDto> res = await stockService.GetBranchStockAsync(tenantNumber, query, ct);

            return Results.Ok(res);

        });
    }
}
