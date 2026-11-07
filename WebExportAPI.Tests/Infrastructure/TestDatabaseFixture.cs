namespace WebExportAPI.Tests.Infrastructure;

public sealed class TestDatabaseFixture : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await using var db = TestDbContextFactory.CreateContext();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}