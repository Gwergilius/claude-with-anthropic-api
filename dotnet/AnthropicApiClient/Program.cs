using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnthropicApiClient;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Build configuration from appsettings.json, environment-specific settings and User Secrets
            // Use Production as default to match ASP.NET Core WebHost/HostBuilder behavior
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<Program>()
                .Build();

            // Setup dependency injection container using Startup
            var services = new ServiceCollection();
            
            var startup = new Startup(configuration);
            startup.ConfigureServices(services);

            // Build service provider
            using var serviceProvider = services.BuildServiceProvider();
            
            // Get the application from DI container and run it
            var application = serviceProvider.GetRequiredService<Application>();
            await application.Run();
        }
        catch (Exception ex)
        {
            // Since we can't inject logger before we build the service provider in Main,
            // we use Console.WriteLine as fallback for startup errors
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine("Make sure your API key is valid and you have credits available.");
        }
    }
}
