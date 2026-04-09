using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnthropicApiClient;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Configure logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
        });
        
        // Configure options
        services.Configure<AnthropicOptions>(
            _configuration.GetSection(AnthropicOptions.SectionName));
        
        // Register HttpClientFactory
        services.AddHttpClient();
        
        // Register AnthropicClient as a service
        services.AddTransient<IAntropicClient, AnthropicClient>();
        
        // Register Application
        services.AddTransient<Application>();
    }
}