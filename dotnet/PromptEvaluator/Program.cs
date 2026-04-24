
var builder = Host.CreateApplicationBuilder(args);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var host = builder.Build();
startup.Configure(host);

await host.Run<Application>();
