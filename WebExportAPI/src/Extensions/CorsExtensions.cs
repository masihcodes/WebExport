namespace WebExportAPI.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCustomCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("BlazorClient", policy =>
            {
                policy
                    .WithOrigins("https://localhost:4001", "http://localhost:4000")
                    .WithMethods("GET", "POST")
                    .AllowAnyHeader()
                    .WithExposedHeaders("Retry-After", "ETag", "X-Request-ID");
            });

            // options.AddPolicy("AllowAll", policy =>
            //     {
            //         policy
            //             .AllowAnyOrigin()
            //             .AllowAnyMethod()
            //             .AllowAnyHeader();
            //     });
        });
        return services;
    }
}
