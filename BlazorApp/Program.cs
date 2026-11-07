using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorApp;

using Soenneker.Quark;
using Soenneker.Quark.Gen.Lucide.Generated;
using Blazor.Sonner.Extensions;

using BlazorApp.Services;




var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri("http://localhost:3000/") });
builder.Services.AddScoped<Session>();

builder.Services.AddQuarkSuiteAsScoped();
builder.Services.AddLucideIconsAsScoped();
builder.Services.AddSonner();


await builder.Build().RunAsync();
