namespace PromptEvaluator.Extensions;

public static class HostExtensions
{
    extension(IHost self)
    {
        public Task Run<TApplication>() where TApplication : Application
            => self.Services.GetRequiredService<TApplication>().Run();
    }
}
