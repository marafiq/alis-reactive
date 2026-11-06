using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddJsonOptions(json => json.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)))
    .AddRazorOptions(razor =>
    {
        razor.ViewLocationFormats.Clear();
        razor.ViewLocationFormats.Add("/{1}/{0}.cshtml");
        razor.ViewLocationFormats.Add("/Shared/{0}.cshtml");
    });
builder.Services.AddHostedService<TestHostWatch>();

var app = builder.Build();

RequireBuilt(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "js", "alis-poc.js"));

app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("pages", "{controller=Home}/{action=Index}");
app.Run();

static void RequireBuilt(string bundle)
{
    if (!File.Exists(bundle))
        throw new InvalidOperationException($"'{bundle}' is not built. Build it: poc/build.sh.");
}

public partial class Program;

internal sealed class TestHostWatch(IConfiguration configuration, IHostApplicationLifetime lifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!int.TryParse(configuration["alis-test-host"], out var hostId)) return;
        if (IsRunning(hostId, out var host))
        {
            using (host)
                await host.WaitForExitAsync(stoppingToken);
        }

        lifetime.StopApplication();
    }

    private static bool IsRunning(int processId, out Process process)
    {
        try
        {
            process = Process.GetProcessById(processId);
            return true;
        }
        catch (ArgumentException)
        {
            process = Process.GetCurrentProcess();
            return false;
        }
    }
}
