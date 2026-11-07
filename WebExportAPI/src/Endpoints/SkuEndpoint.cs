using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs;
using WebExportAPI.Services;

namespace WebExportAPI.Endpoints;

public static class SkuEndpoint
{
    public static void MapSkuRoute(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/").RequireAuthorization();

        group.MapGet("skus/{sku}", async ([FromServices] ISkuService skuService, ClaimsPrincipal user, CancellationToken ct, [FromRoute] string sku, [FromQuery] short? branch = null) =>
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

            if (branch is short branchNumber && !await skuService.BranchExistsAsync(tenantNumber, branchNumber, ct))
            {
                return Results.BadRequest(new ErrorResponseDto
                {
                    ErrorCode = "INVALID_PARAMETER",
                    Message = "Die angegebene Filiale wurde nicht gefunden.",
                    Timestamp = DateTime.UtcNow
                });
            }

            var res = await skuService.GetSkuAsync(ct, tenantNumber, sku, branch);

            return (res is null) ?
             Results.NotFound(new ErrorResponseDto
             {
                 ErrorCode = "NOT_FOUND",
                 Message = "Die SKU wurde nicht gefunden.",
                 Timestamp = DateTime.UtcNow
             })
            : Results.Ok(res);
        });
    }
}
