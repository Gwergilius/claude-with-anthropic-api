using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorChat;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<BlazorChat.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);


await builder.Build().RunAsync();
