using System.Diagnostics;
using System.Globalization;
using Alis.Reactive.Conformance.App.World;

namespace Alis.Reactive.Conformance.App;

internal static class Program
{
    private const string TestHost = "test-host";

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        if (builder.Environment.IsDevelopment()) builder.Services.AddWorld();

        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseWorld();
            app.MapWorld();
        }

        if (app.Configuration[TestHost] is { } testHost) StopWith(testHost, app.Lifetime);
        app.Run();
    }

    private static void StopWith(string testHost, IHostApplicationLifetime lifetime)
        => _ = Process.GetProcessById(int.Parse(testHost, CultureInfo.InvariantCulture)).WaitForExitAsync().ContinueWith(_ => lifetime.StopApplication(), TaskScheduler.Default);
}
