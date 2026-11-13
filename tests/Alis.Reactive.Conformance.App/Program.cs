using System.Diagnostics;
using System.Globalization;
using Alis.Reactive.Conformance.App.World;

namespace Alis.Reactive.Conformance.App;

public sealed class Program
{
    private const string TestHost = "test-host";
    private const string Host = "host";

    private Program()
    {
    }

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var host = builder.Configuration[Host] ?? throw new InvalidOperationException("The conformance app starts under a host spec/Reference/app.json defines: --host <name>.");
        builder.Services.AddControllersWithViews().AddJsonOptions(json => AppHost.SetJsonOptions(host, json.JsonSerializerOptions));
        builder.Services.AddDistributedMemoryCache();
        if (builder.Environment.IsDevelopment()) builder.Services.AddWorld();

        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseWorld();
            app.MapWorld();
        }

        app.UseStaticFiles();
        app.MapControllers();
        if (app.Configuration[TestHost] is { } testHost) StopWith(testHost, app.Lifetime);
        app.Run();
    }

    private static void StopWith(string testHost, IHostApplicationLifetime lifetime)
        => _ = Process.GetProcessById(int.Parse(testHost, CultureInfo.InvariantCulture)).WaitForExitAsync().ContinueWith(_ => lifetime.StopApplication(), TaskScheduler.Default);
}
