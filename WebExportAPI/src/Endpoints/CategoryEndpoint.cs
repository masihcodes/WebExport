using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DTOs;
using Models.DTOs.CategoryResponse;
using WebExportAPI.Db;

namespace WebExportAPI.Endpoints;

public static class CategoryEndpoint
{
    public static void MapCategoryRoutes(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/").RequireAuthorization();

        group.MapGet("categories", async ([FromServices] AppDbContext db, ClaimsPrincipal user, CancellationToken ct, [FromQuery] bool activeOnly = false, [FromQuery] bool asTree = false) =>
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

            IQueryable<Category> resQuery = db.Categories.AsNoTracking()
            .Where(category => category.TenantNumber == tenantNumber)
            .OrderBy(category => category.Parent == null ? (int?)null : category.Parent.Number)
            .ThenBy(category => category.Rank)
            .ThenBy(category => category.Number);

            if (activeOnly)
            {
                resQuery = resQuery.Where(category => category.IsActive);
            }

            var categories = await resQuery.Select(category => new CategoryResponseDto
            {
                Number = category.Number,
                ParentNumber = category.Parent == null ? (int?)null : category.Parent.Number,
                Rank = category.Rank,
                Name = category.Name,
                SearchTerms = category.SearchTerms,
                IsActive = category.IsActive
            }).ToArrayAsync(ct);

            if (asTree)
            {
                var tree = BuildCategoryTree(categories);
                return Results.Ok(tree);
            }

            return Results.Ok(categories);
        });

        group.MapGet("categories/{number}", async ([FromServices] AppDbContext db, ClaimsPrincipal user, CancellationToken ct, [FromRoute] int number) =>
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

            IQueryable<Category> resQuery = db.Categories.AsNoTracking()
            .Where(category => category.TenantNumber == tenantNumber && category.Number == number);

            var categories = await resQuery.Select(category => new CategoryResponseDto
            {
                Number = category.Number,
                ParentNumber = category.Parent == null ? (int?)null : category.Parent.Number,
                Rank = category.Rank,
                Name = category.Name,
                SearchTerms = category.SearchTerms,
                IsActive = category.IsActive
            }).FirstOrDefaultAsync(ct);

            if (categories is null)
            {
                return Results.NotFound(new ErrorResponseDto
                {
                    ErrorCode = "NOT_FOUND",
                    Message = "Die Kategorie wurde nicht gefunden.",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Results.Ok(categories);
        });
    }




    private static List<CategoryTreeResponseDto> BuildCategoryTree(IReadOnlyCollection<CategoryResponseDto> categories)
    {
        var nodes = categories.ToDictionary(category => category.Number, category => new CategoryTreeResponseDto
        {
            Number = category.Number,
            Name = category.Name,
            Rank = category.Rank,
            IsActive = category.IsActive
        });

        var roots = new List<CategoryTreeResponseDto>();

        foreach (var category in categories.OrderBy(category => category.Rank).ThenBy(category => category.Number))
        {
            var node = nodes[category.Number];

            if (category.ParentNumber is null)
            {
                roots.Add(node);
            }
            else if (nodes.TryGetValue(category.ParentNumber.Value, out var parent))
            {
                parent.Children.Add(node);
            }
        }

        return roots;
    }
}
