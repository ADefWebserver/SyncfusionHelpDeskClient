using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Syncfusion.Blazor;
using SyncfusionHelpDeskClient.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("SyncfusionHelpDeskClient.ServerAPI", 
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Supply HttpClient instances that include access tokens
// when making requests to the server project
builder.Services.AddScoped(sp => 
sp.GetRequiredService<IHttpClientFactory>()
.CreateClient("SyncfusionHelpDeskClient.ServerAPI"));

// This allows anonymous requests.
// See: https://bit.ly/2Y3ET3K
builder.Services.AddHttpClient("ServerAPI.NoAuthenticationClient",
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

builder.Services.AddApiAuthorization();

// Syncfusion support
builder.Services.AddSyncfusionBlazor(options => { options.IgnoreScriptIsolation = false; });

await builder.Build().RunAsync();