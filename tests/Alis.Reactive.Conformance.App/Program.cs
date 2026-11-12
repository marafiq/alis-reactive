using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.App.World;

namespace Alis.Reactive.Conformance.App;

public sealed class Program
{
    private const string TestHost = "test-host";

    private Program()
    {
    }

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllersWithViews().AddJsonOptions(json => json.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
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
