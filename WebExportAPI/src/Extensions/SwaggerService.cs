using Microsoft.OpenApi;

namespace WebExportAPI.Extensions;

public static class SwaggerService
{
    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.UseInlineDefinitionsForEnums();
            // options.OrderActionsBy((action) => action.RelativePath);
            options.AddSecurityDefinition("Bearer",new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    Description = "Enter JWT access token.",
                }
            );

            options.AddSecurityDefinition("ApiKey",new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Name = "X-API-KEY",
                    Description = "Enter API key.",
                }
            );

            options.AddSecurityRequirement(document =>
            {
                return new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
                    [new OpenApiSecuritySchemeReference("ApiKey", document)] = [],
                };
            });
        });
    }
}
