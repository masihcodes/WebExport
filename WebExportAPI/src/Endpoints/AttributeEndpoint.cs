using Microsoft.EntityFrameworkCore;
using Models.DTOs.AttributeTypeResponse;
using WebExportAPI.Db;

namespace WebExportAPI.Endpoints;

public static class AttributeEndpoint
{
    public static void MapAttributeRoute(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/").RequireAuthorization();

        group.MapGet("attribute-types", async (AppDbContext db, CancellationToken ct) =>
        {
            var res = await db.AttributeTypes.Where(a => a.IsWebShopRelevant).Select(a => new AttributeTypeResponseDto
            {
                Name = a.Name,
                Number = a.Number,
                Attributes = a.Attributes.Select(at => new AttributeResponseDto
                {
                    Name = at.Name,
                    Number = at.Number
                }).ToList()
            })
            .ToListAsync(ct);

            return Results.Ok(res);
        });
    }
}
