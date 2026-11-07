using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Models.DTOs.Auth;
using WebExportAPI.Db;

namespace WebExportAPI.Services;


public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto client, CancellationToken ct);
}


public class AuthService(AppDbContext db, IConfiguration configuration) : IAuthService
{
    public async Task<AuthResponseDto?> LoginAsync(LoginDto client, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(client.ClientId) || string.IsNullOrWhiteSpace(client.ClientSecret))
        {
            return null;
        }

        ApiClient? res = await db.ApiClients.Include(x => x.Tenant).Where(x => x.ClientId == client.ClientId).FirstOrDefaultAsync(ct);

        if (res is null || !res.IsActive || !res.Tenant.IsActive)
        {
            return null;
        }

        var hasher = new PasswordHasher<ApiClient>();

        PasswordVerificationResult result = hasher.VerifyHashedPassword(res, res.SecretHash, client.ClientSecret);

        if (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            return GenerateToken(res);
        }
        else
        {
            return null;
        }
    }

    private AuthResponseDto GenerateToken(ApiClient res)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, res.TenantNumber.ToString()),
            new Claim("name", res.Tenant.Name.ToString()),
            new Claim("tenant", res.TenantNumber.ToString())
        };

        var expires = DateTime.UtcNow.AddMinutes(int.Parse(configuration["Jwt:ExpiryMinutes"]!));

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new AuthResponseDto
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            TokenType = "Bearer",
            TenantNumber = res.TenantNumber,
            ExpiresIn = 3600,
            ExpiresAt = expires

        };
    }
}
