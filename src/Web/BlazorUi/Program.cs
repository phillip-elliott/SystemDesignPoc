using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorUi;
using BlazorUi.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register Typed HTTP Clients pointing to local base address (NGINX will route /api/read and /api/write)
builder.Services.AddHttpClient<ProductApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

builder.Services.AddHttpClient<ProductCommandClient>(client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

await builder.Build().RunAsync();