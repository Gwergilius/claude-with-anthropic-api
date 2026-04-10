var builder = WebApplication.CreateBuilder(args);

var startup = new BlazorChat.Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<BlazorChat.App>()
   .AddInteractiveServerRenderMode();

app.Run();
