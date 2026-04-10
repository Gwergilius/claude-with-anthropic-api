using AnthropicShared;

namespace BlazorChat;

public class Startup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRazorComponents()
                .AddInteractiveServerComponents();

        services.AddHttpClient();
        services.AddOptions<AnthropicOptions>()
            .Bind(configuration.GetSection(AnthropicOptions.SectionName))
            .ValidateOnStart();

        // Register as Singleton so conversation context persists for the lifetime of the app
        services.AddSingleton<IAntropicClient, AnthropicClient>();
    }
}
