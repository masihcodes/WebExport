using Microsoft.EntityFrameworkCore;
using Models.Enums;
using Npgsql;
using WebExportAPI.Db;

namespace WebExportAPI.Tests.Infrastructure;

internal static class TestDbContextFactory
{
    public static AppDbContext CreateContext()
    {
        var connectionString = TestConfiguration.GetConnectionString();
        var connectionSettings = new NpgsqlConnectionStringBuilder(connectionString);

        if (connectionSettings.Database != "webexport_automated_tests")
            throw new InvalidOperationException("Tests must use the webexport_automated_tests database.");


        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MapEnum<DeliveryStatus>("delivery_status");
                npgsql.MapEnum<TextType>("text_type");
                npgsql.MapEnum<RelationType>("relation_type");
                npgsql.MapEnum<ImageType>("image_type");
                npgsql.MapEnum<UnitOfMeasure>("unit_of_measure");
            })
            .Options;

        return new AppDbContext(options);
    }
}