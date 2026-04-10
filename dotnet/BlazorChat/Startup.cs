using AnthropicShared;

namespace BlazorChat;

public class Startup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Logging is fully handled by WebAssemblyHostBuilder.CreateDefault:
        // it registers WebAssemblyConsoleLoggerProvider and applies log-level settings
        // from appsettings.json automatically — no explicit AddLogging call needed here.

        services.AddHttpClient();
        services.AddOptions<AnthropicOptions>()
            .Bind(configuration.GetSection(AnthropicOptions.SectionName))
            .ValidateOnStart();

        // Register as Singleton so conversation context persists for the lifetime of the app
        services.AddSingleton<IAntropicClient, AnthropicClient>();
    }
}
