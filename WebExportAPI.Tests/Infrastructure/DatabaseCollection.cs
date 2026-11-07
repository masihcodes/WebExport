namespace WebExportAPI.Tests.Infrastructure;

[CollectionDefinition("Database tests")]
public class DatabaseCollection : ICollectionFixture<TestDatabaseFixture>
{
}