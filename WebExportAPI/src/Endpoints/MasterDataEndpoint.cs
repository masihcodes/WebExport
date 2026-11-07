using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models.DTOs;
using Models.DTOs.ArticleResponse;
using Models.DTOs.MasterDataResponse;
using WebExportAPI.Db;
using WebExportAPI.Helpers;

namespace WebExportAPI.Endpoints;

public static class MasterDataEndpoint
{
    public static void MapMasterDataRoutes(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/").RequireAuthorization();

        // group.MapGet("colors", async (AppDbContext db, CancellationToken ct) =>
        // {
        //     var res = await db.Colors.OrderBy(color => color.Id).Select(color => new ColorDto
        //     {
        //         Id = color.Id,
        //         Name = color.Name,
        //         Blue = color.Blue,
        //         Green = color.Green,
        //         Red = color.Red
        //     })
        //     .ToArrayAsync(ct);

        //     return Results.Ok(res);
        // });

        group.MapGet("colors", async (AppDbContext db, HttpContext context, IOptions<JsonOptions> jsonOptions, CancellationToken ct) =>
        {
            var res = await db.Colors
                .OrderBy(color => color.Id)
                .Select(color => new ColorDto
                {
                    Id = color.Id,
                    Name = color.Name,
                    Blue = color.Blue,
                    Green = color.Green,
                    Red = color.Red
                })
                .ToArrayAsync(ct);

            byte[] content = JsonSerializer.SerializeToUtf8Bytes(
                res, jsonOptions.Value.SerializerOptions);

            var entityTag = EntityTagHelper.Create(content);

            context.Response.Headers.CacheControl = "private, no-cache";

            return Results.Bytes(
                content,
                contentType: "application/json; charset=utf-8",
                entityTag: entityTag);
        })
        .Produces<ColorDto[]>(StatusCodes.Status200OK, "application/json")
        .Produces(StatusCodes.Status304NotModified);

        group.MapGet("sizes", async (AppDbContext db, CancellationToken ct) =>
        {
            var res = await db.Sizes
            .OrderBy(size => size.SizeRange)
            .ThenBy(size => size.SortOrder)
            .ThenBy(size => size.Id)
            .Select(size => new SizeResponseDto
            {
                Id = size.Id,
                Name = size.Name,
                SizeRange = size.SizeRange,
                SortOrder = size.SortOrder
            })
            .ToArrayAsync(ct);

            return Results.Ok(res);
        });

        group.MapGet("branches", async (AppDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
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

            var res = await db.Branches.Where(b => b.TenantNumber == tenantNumber)
            .OrderBy(b => b.Number)
            .ThenBy(b => b.Name)
            .Select(b => new BranchResponseDto
            {
                Name = b.Name,
                Number = b.Number,
                City = b.City,
                IsActive = b.IsActive
            })
            .ToArrayAsync(ct);

            return Results.Ok(res);
        });

        group.MapGet("delivery-status", async (AppDbContext db, CancellationToken ct) =>
        {
            var res = await db.DeliveryStatusTexts
            .OrderBy(ds => ds.Status)
            .Select(ds => new DeliveryStatusResponseDto
            {
                Text = ds.Text,
                Status = ds.Status
            })
            .ToArrayAsync(ct);

            return Results.Ok(res);
        });
    }
}
