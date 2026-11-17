using System.Diagnostics;
using System.Globalization;
using Alis.Reactive.Conformance.App.Bench;
using Alis.Reactive.Conformance.App.Care;
using Alis.Reactive.Conformance.App.World;
using Microsoft.AspNetCore.Authentication.Cookies;

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
        AppHost.AddRequests(host, builder.Services);
        AppHost.AddValidation(builder.Services);
        AppHost.AddClassNames(host, builder.Services);
        AppHost.AddRealtime(host, builder.Services);
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(cookie => cookie.LoginPath = SessionNotesEndpoints.SignIn);
        if (builder.Environment.IsDevelopment()) builder.Services.AddWorld();
        AppHost.AddTracing(host, builder);

        var app = builder.Build();
        app.UseRequestCultures();
        app.UseRequestZones();
        if (app.Environment.IsDevelopment())
        {
            app.UseWorld();
            app.UseWorldLines();
            app.MapWorld();
        }

        app.UseStaticFiles();
        AppHost.UseTracing(host, app);
        app.MapControllers();
        app.MapLiveLines();
        app.MapBenchLine();
        if (app.Configuration[TestHost] is { } testHost) StopWith(testHost, app.Lifetime);
        app.Run();
    }

    private static void StopWith(string testHost, IHostApplicationLifetime lifetime)
        => _ = Process.GetProcessById(int.Parse(testHost, CultureInfo.InvariantCulture)).WaitForExitAsync().ContinueWith(_ => lifetime.StopApplication(), TaskScheduler.Default);
}
