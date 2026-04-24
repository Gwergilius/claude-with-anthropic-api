using PromptEvaluator.Models;
using PromptEvaluator.Services;

namespace PromptEvaluator;

public class Startup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions<AnthropicOptions>()
            .Bind(configuration.GetSection(AnthropicOptions.SectionName))
            .ValidateOnStart();

        services.AddOptions<EvaluatorOptions>()
            .Bind(configuration.GetSection(EvaluatorOptions.SectionName));

        services.AddHttpClient();

        services.AddSingleton<IAnthropicRequestTelemetry, NullAnthropicRequestTelemetry>();

        // Transient so each scope created by PromptEvaluatorService gets a fresh, context-free client.
        services.AddTransient<IAntropicClient, AnthropicClient>();

        services.AddTransient<PromptEvaluatorService>();
        services.AddTransient<Application>();
    }

    public void Configure(IHost host)
    {
        // No middleware pipeline for console apps.
    }
}
