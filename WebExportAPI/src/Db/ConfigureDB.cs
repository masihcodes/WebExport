using Microsoft.EntityFrameworkCore;
using Models.Enums;

namespace WebExportAPI.Db;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDb(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");

        services.AddDbContextPool<AppDbContext>(options =>
        {
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MapEnum<DeliveryStatus>("delivery_status");
                npgsql.MapEnum<TextType>("text_type");
                npgsql.MapEnum<RelationType>("relation_type");
                npgsql.MapEnum<ImageType>("image_type");
                npgsql.MapEnum<UnitOfMeasure>("unit_of_measure");
            });
        });

        return services;
    }
}