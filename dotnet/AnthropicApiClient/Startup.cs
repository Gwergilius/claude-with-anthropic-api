
namespace AnthropicApiClient;

public class Startup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Configure logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddConfiguration(configuration.GetSection("Logging"));
        });
        
        // Configure options
        services.AddOptions<AnthropicOptions>()
            .Bind(configuration.GetSection(AnthropicOptions.SectionName))
            .ValidateOnStart();
        
        // Register HttpClientFactory
        services.AddHttpClient();

        services.AddSingleton<IAnthropicRequestTelemetry, NullAnthropicRequestTelemetry>();
        
        // Register AnthropicClient as a service
        services.AddTransient<IAntropicClient, AnthropicClient>();
        
        // Register Application
        services.AddTransient<Application>();
    }
}