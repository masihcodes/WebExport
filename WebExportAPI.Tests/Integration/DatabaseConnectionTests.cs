using Microsoft.EntityFrameworkCore;
using WebExportAPI.Tests.Infrastructure;

namespace WebExportAPI.Tests.Integration;



[Collection("Database tests")]
public class DatabaseConnectionTests
{
    [Fact]
    public async Task CanConnectAsync_TestDatabase_ReturnsTrue()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();

        // Act
        bool canConnect = await db.Database.CanConnectAsync();

        // Assert
        Assert.True(canConnect, "Could not connect to the automated test database.");
    }
}