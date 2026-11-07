using Microsoft.Extensions.Configuration;

namespace WebExportAPI.Tests.Infrastructure;

internal static class TestConfiguration
{
    public static string GetConnectionString()
    {
        var configuration = new ConfigurationBuilder().AddUserSecrets(typeof(TestConfiguration).Assembly).Build();
        return configuration.GetConnectionString("TestConnection") ?? throw new InvalidOperationException("The test connection string is not configured.");
    }
}