using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Models.DTOs;
using WebExportAPI.Services;

namespace WebExportAPI.Extensions;

public static class AuthExtensions
{
    public static void AddJwt(this IServiceCollection service, IConfiguration config)
    {
        service.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                ValidateIssuer = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = config["Jwt:Audience"],
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = "role"
            };

            options.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    context.HandleResponse();

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.Headers["WWW-Authenticate"] = "Bearer";

                    await context.Response.WriteAsJsonAsync(new ErrorResponseDto
                    {
                        ErrorCode = "UNAUTHORIZED",
                        Message = "Das Zugriffstoken fehlt, ist abgelaufen oder ungültig."
                    });
                },
                OnTokenValidated = context =>
                {
                    bool isValidTenant = short.TryParse(context.Principal?.FindFirstValue("tenant"), out _);
                    if (!isValidTenant)
                    {
                        context.Fail("Das Token enthält keine Mandantennummer.");
                    }
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.TryGetValue("access_token", out var token))
                    {
                        context.Token = token;
                    }
                    return Task.CompletedTask;
                },
                // OnMessageReceived = async context =>
                // {
                //     var request = context.Request;

                //     if (!request.HasJsonContentType() || request.ContentLength == 0)
                //     {
                //         return;
                //     }

                //     request.EnableBuffering();

                //     try
                //     {
                //         using var document = await JsonDocument.ParseAsync(
                //             request.Body,
                //             cancellationToken: context.HttpContext.RequestAborted);

                //         var root = document.RootElement;

                //         if (root.ValueKind == JsonValueKind.Object &&
                //             root.TryGetProperty("access_token", out var tokenElement) &&
                //             tokenElement.ValueKind == JsonValueKind.String)
                //         {
                //             context.Token = tokenElement.GetString();
                //         }
                //     }
                //     catch (JsonException)
                //     {
                //         context.Fail("Die Anfrage enthält ungültiges JSON.");
                //     }
                //     finally
                //     {
                //         request.Body.Position = 0;
                //     }
                // }
            };
        });

        service.AddAuthorization();
        service.AddScoped<IAuthService, AuthService>();
    }
}
