using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs;
using Models.DTOs.ArticleResponse;
using WebExportAPI.Services;

namespace WebExportAPI.Endpoints;

public static class ArticleEndPoint
{
    public static RouteGroupBuilder MapArticleRoutes(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/");

        group.MapPost("articles", async (
                                                    [FromServices] IArticleService articleService,
                                                    ClaimsPrincipal user,
                                                    CancellationToken ct,
                                                    [FromBody] ArticleListQueryDto? query) =>
        {
            // var tenaName = httpContextAccessor.HttpContext?.User.Identity?.Name;
            // var tenantClaim = user.FindFirstValue("tenant");

            query ??= new ArticleListQueryDto();

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

            if (query.Page < 1 || query.PageSize < 1 || query.PageSize > 200)
            {
                return Results.BadRequest(new ErrorResponseDto
                {
                    ErrorCode = "INVALID_PARAMETER",
                    Message = "page muss mindestens 1 sein. pageSize muss zwischen 1 und 200 liegen.",
                    Timestamp = DateTime.UtcNow
                });
            }

            if (query.Branch is short branchNumber && !await articleService.BranchExistsAsync(tenantNumber, branchNumber, ct))
            {
                return Results.BadRequest(new ErrorResponseDto
                {
                    ErrorCode = "INVALID_PARAMETER",
                    Message = "Die angegebene Filiale wurde nicht gefunden.",
                    Timestamp = DateTime.UtcNow
                });
            }

            PagedResponseDto<ArticleResponseDto> response = await articleService.GetArticlesAsync(tenantNumber, query, ct);

            return Results.Ok(response);
        });



        group.MapGet("articles/{articleNumber}", async (
                                                                    [FromServices] IArticleService articleService,
                                                                    ClaimsPrincipal user,
                                                                    CancellationToken ct,
                                                                    [FromRoute] string articleNumber,
                                                                    [FromQuery] short? branch = null) =>
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

            if (branch is short branchNumber && !await articleService.BranchExistsAsync(tenantNumber, branchNumber, ct))
            {
                return Results.BadRequest(new ErrorResponseDto
                {
                    ErrorCode = "INVALID_PARAMETER",
                    Message = "Die angegebene Filiale wurde nicht gefunden.",
                    Timestamp = DateTime.UtcNow
                });
            }

            var article = await articleService.GetArticleAsync(ct, tenantNumber, articleNumber, branch);

            if (article is null)
            {
                return Results.NotFound(new ErrorResponseDto
                {
                    ErrorCode = "NOT_FOUND",
                    Message = "Der Artikel wurde nicht gefunden.",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Results.Ok(article);
        });

        return group;
    }
}
