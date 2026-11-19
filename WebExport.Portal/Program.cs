using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WebExport.Portal;
using Blazor.Sonner.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var BaseAddress = config["ApiUrl"] ?? "http://localhost:3000/";
    return new HttpClient { BaseAddress = new Uri(BaseAddress) };
});

builder.Services.AddSonner();

builder.Services.AddScoped<Session>();

await builder.Build().RunAsync();
