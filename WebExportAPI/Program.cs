using System.Text.Json;
using System.Text.Json.Serialization;
using WebExportAPI.Db;
using WebExportAPI.Endpoints;
using WebExportAPI.Extensions;
using WebExportAPI.Middleware;
using WebExportAPI.Services;


var builder = WebApplication.CreateBuilder(args);



// enum converter
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
// this is required to throw an exception when the route parameters are invalid
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

builder.Services.AddDb(builder.Configuration);
builder.Services.AddJwt(builder.Configuration);
builder.Services.AddApiRateLimiting(); // add rate limiting
builder.Services.ConfigureSwagger();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<ISkuService, SkuService>();
builder.Services.AddScoped<IStockService, StockService>();

builder.Services.AddCustomCors(); // add cors

builder.Services.AddValidation();


WebApplication app = builder.Build();

app.UseRouting();
app.UseCors("BlazorClient"); // add cors
app.UseAuthentication();
app.UseAuthorization();

// app.AddErrorHandler();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRateLimiter(); // add rate limiting

app.MapStatusRoute();
app.MapTenantRoute();
app.MapArticleRoutes().RequireAuthorization();
app.MapSkuRoute();
app.MapStockRoutes();
app.MapAttributeRoute();
app.MapMasterDataRoutes();
app.MapCategoryRoutes();
app.MapAuthRoute().RequireRateLimiting(RateLimitingExtensions.LoginPolicy);

app.Run();
