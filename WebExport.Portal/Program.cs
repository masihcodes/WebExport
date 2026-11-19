using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WebExport.Portal;
using Blazor.Sonner.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri("http://localhost:3000/") });
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri("https://webexport.onrender.com/") });

builder.Services.AddSonner();

builder.Services.AddScoped<Session>();

await builder.Build().RunAsync();
