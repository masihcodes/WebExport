using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Models.DTOs;
using Models.DTOs.TenantResponse;
using WebExportAPI.Db;

namespace WebExportAPI.Endpoints;

public static class TenantEndpoint
{
    public static void MapTenantRoute(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/").RequireAuthorization();

        group.MapGet("tenant", async (AppDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
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

            TenantResponseDto? res = await db.Tenants
            .Where(tenant => tenant.TenantNumber == tenantNumber)
            .Select(tenant => new TenantResponseDto
            {
                Number = tenant.TenantNumber,
                Name = tenant.Name,
                BranchCount = db.Branches.Count(branch => branch.TenantNumber == tenant.TenantNumber)
            }).FirstOrDefaultAsync(ct);

            if (res is not null)
            {
                return Results.Ok(res);
            }

            return Results.NotFound(new ErrorResponseDto
            {
                ErrorCode = "NOT_FOUND",
                Message = "Der Mandant wurde nicht gefunden.",
                Timestamp = DateTime.UtcNow
            });
        });
    }
}
