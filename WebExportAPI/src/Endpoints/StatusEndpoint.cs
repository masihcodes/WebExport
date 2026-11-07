using Models.DTOs;
using Models.DTOs.StatusResponse;
using WebExportAPI.Db;

namespace WebExportAPI.Endpoints;


public static class StatusEndpoints
{
    public static void MapStatusRoute(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/v1/status", async (AppDbContext db, CancellationToken ct) =>
        {
            bool res = await db.Database.CanConnectAsync(ct);
            if (res)
            {
                return Results.Ok(new StatusResponseDto
                {
                    Status = "OK",
                    DatabaseAvailable = true,
                    Version = "1.0.0",
                    Timestamp = DateTime.UtcNow
                });
            }
            return Results.Json(
                new ErrorResponseDto
                {
                    ErrorCode = "DATABASE_UNAVAILABLE",
                    Message = "Die Datenbank ist nicht erreichbar.",
                    Timestamp = DateTime.UtcNow
                },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        });
    }
}
