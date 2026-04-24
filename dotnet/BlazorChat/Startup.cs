using BlazorChat.Services;

namespace BlazorChat;

public class Startup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRazorComponents()
                .AddInteractiveServerComponents();

        services.AddHttpClient();

        services.AddSingleton(_ =>
        {
            var o = new AnthropicOptions();
            configuration.GetSection(AnthropicOptions.SectionName).Bind(o);

            var results = new List<ValidationResult>();
            var ctx = new ValidationContext(o);
            if (!Validator.TryValidateObject(o, ctx, results, validateAllProperties: true))
            {
                var msg = string.Join("; ", results.Select(r => r.ErrorMessage));
                throw new OptionsValidationException(
                    Options.DefaultName,
                    typeof(AnthropicOptions),
                    [msg]);
            }

            return new RuntimeAnthropicOptions(o);
        });
        services.AddSingleton<IOptions<AnthropicOptions>>(sp => sp.GetRequiredService<RuntimeAnthropicOptions>());
        services.AddSingleton<IOptionsMonitor<AnthropicOptions>>(sp => sp.GetRequiredService<RuntimeAnthropicOptions>());

        services.AddScoped<IAnthropicUserSettingsService, AnthropicUserSettingsService>();

        services.AddSingleton<AnthropicRequestLog>();
        services.AddSingleton<IAnthropicRequestTelemetry>(sp => sp.GetRequiredService<AnthropicRequestLog>());

        // Register as Singleton so conversation context persists for the lifetime of the app
        services.AddSingleton<IAntropicClient, AnthropicClient>();

        services.AddScoped<IAnthropicStreamProgressService, AnthropicStreamProgressService>();
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<BlazorChat.App>()
           .AddInteractiveServerRenderMode();
    }
}
