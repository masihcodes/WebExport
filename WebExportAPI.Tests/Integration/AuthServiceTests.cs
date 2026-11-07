using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Models.DTOs.Auth;
using WebExportAPI.Db;
using WebExportAPI.Services;
using WebExportAPI.Tests.Infrastructure;

namespace WebExportAPI.Tests.Integration;

[Collection("Database tests")]
public class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsValidToken()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        const string clientSecret = "Test-Client-Secret-123!";
        const string signingKey = "webexport-tests-only-signing-key-0123456789";

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = signingKey,
            ["Jwt:Issuer"] = "WebExportAPI.Tests",
            ["Jwt:Audience"] = "WebExportAPI.Tests.Client",
            ["Jwt:ExpiryMinutes"] = "60"
        }).Build();

        var tenant = new Tenant { TenantNumber = 1, Name = "Testfirma 1", IsActive = true };

        var client = new ApiClient
        {
            Tenant = tenant,
            TenantNumber = tenant.TenantNumber,
            ClientId = "test-client-1",
            Name = "Testclient",
            SecretHash = string.Empty,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var hasher = new PasswordHasher<ApiClient>();
        client.SecretHash = hasher.HashPassword(client, clientSecret);

        db.Tenants.Add(tenant);
        db.ApiClients.Add(client);

        await db.SaveChangesAsync();

        var service = new AuthService(db, configuration);

        var login = new LoginDto
        {
            ClientId = client.ClientId,
            ClientSecret = clientSecret
        };

        // Act
        var result = await service.LoginAsync(login, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal(1, result.TenantNumber);
        Assert.Equal(3600, result.ExpiresIn);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));

        var handler = new JwtSecurityTokenHandler
        {
            MapInboundClaims = false
        };

        var principal = handler.ValidateToken(result.AccessToken, new TokenValidationParameters
        {
            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ValidateIssuer = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = configuration["Jwt:Audience"],
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero
        },
            out _);

        Assert.Equal("1", principal.FindFirst("tenant")?.Value);
    }


    [Fact]
    public async Task LoginAsync_InvalidCredentials_ReturnsNull()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        const string clientSecret = "Test-Client-Secret-123!";
        const string signingKey = "webexport-tests-only-signing-key-0123456789";

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = signingKey,
            ["Jwt:Issuer"] = "WebExportAPI.Tests",
            ["Jwt:Audience"] = "WebExportAPI.Tests.Client",
            ["Jwt:ExpiryMinutes"] = "60"
        }).Build();

        var tenant = new Tenant { TenantNumber = 1, Name = "Testfirma 1", IsActive = true };

        var client = new ApiClient
        {
            Tenant = tenant,
            TenantNumber = tenant.TenantNumber,
            ClientId = "test-client-1",
            Name = "Testclient",
            SecretHash = string.Empty,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var hasher = new PasswordHasher<ApiClient>();
        client.SecretHash = hasher.HashPassword(client, clientSecret);

        db.Tenants.Add(tenant);
        db.ApiClients.Add(client);

        await db.SaveChangesAsync();

        var service = new AuthService(db, configuration);

        var login = new LoginDto
        {
            ClientId = client.ClientId,
            ClientSecret = "invalid-secret"
        };

        // Valid credentials
        var validResult = await service.LoginAsync(
            new LoginDto
            {
                ClientId = client.ClientId,
                ClientSecret = clientSecret
            },
            CancellationToken.None);

        Assert.NotNull(validResult);

        // Wrong secret
        var wrongSecretResult = await service.LoginAsync(
            new LoginDto
            {
                ClientId = client.ClientId,
                ClientSecret = "Wrong-Secret-456!"
            },
            CancellationToken.None);

        Assert.Null(wrongSecretResult);

        // Unknown client
        var unknownClientResult = await service.LoginAsync(
            new LoginDto
            {
                ClientId = "unknown-client",
                ClientSecret = clientSecret
            },
            CancellationToken.None);
        Assert.Null(unknownClientResult);

        // Act
        var result = await service.LoginAsync(login, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_InactiveClient_ReturnsNull()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        const string clientSecret = "Test-Client-Secret-123!";
        const string signingKey = "webexport-tests-only-signing-key-0123456789";

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = signingKey,
            ["Jwt:Issuer"] = "WebExportAPI.Tests",
            ["Jwt:Audience"] = "WebExportAPI.Tests.Client",
            ["Jwt:ExpiryMinutes"] = "60"
        }).Build();

        var tenant = new Tenant { TenantNumber = 1, Name = "Testfirma 1", IsActive = true };

        var client = new ApiClient
        {
            Tenant = tenant,
            TenantNumber = tenant.TenantNumber,
            ClientId = "test-client-1",
            Name = "Testclient",
            SecretHash = string.Empty,
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var hasher = new PasswordHasher<ApiClient>();
        client.SecretHash = hasher.HashPassword(client, clientSecret);

        db.Tenants.Add(tenant);
        db.ApiClients.Add(client);

        await db.SaveChangesAsync();

        var service = new AuthService(db, configuration);

        var login = new LoginDto
        {
            ClientId = client.ClientId,
            ClientSecret = clientSecret
        };

        // Act
        var result = await service.LoginAsync(login, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public async Task LoginAsync_InactiveTenant_ReturnsNull()
    {
        // Arrange
        await using var db = TestDbContextFactory.CreateContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        const string clientSecret = "Test-Client-Secret-123!";
        const string signingKey = "webexport-tests-only-signing-key-0123456789";

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = signingKey,
            ["Jwt:Issuer"] = "WebExportAPI.Tests",
            ["Jwt:Audience"] = "WebExportAPI.Tests.Client",
            ["Jwt:ExpiryMinutes"] = "60"
        }).Build();

        var tenant = new Tenant { TenantNumber = 1, Name = "Testfirma 1", IsActive = false };

        var client = new ApiClient
        {
            Tenant = tenant,
            TenantNumber = tenant.TenantNumber,
            ClientId = "test-client-1",
            Name = "Testclient",
            SecretHash = string.Empty,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var hasher = new PasswordHasher<ApiClient>();
        client.SecretHash = hasher.HashPassword(client, clientSecret);

        db.Tenants.Add(tenant);
        db.ApiClients.Add(client);

        await db.SaveChangesAsync();

        var service = new AuthService(db, configuration);

        var login = new LoginDto
        {
            ClientId = client.ClientId,
            ClientSecret = clientSecret
        };

        // Act
        var result = await service.LoginAsync(login, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}