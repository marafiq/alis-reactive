using System.Diagnostics;
using System.Globalization;
using Alis.Reactive.Conformance.App.Bench;
using Alis.Reactive.Conformance.App.Care;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
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
        AppHost.RegisterSyncfusionsLicence(builder.Configuration);
        builder.Services.AddControllersWithViews().AddJsonOptions(json => AppHost.SetJsonOptions(host, json.JsonSerializerOptions));
        builder.Services.AddSingleton(new AdmissionDeskProfile(host == HostNames.Admissions));
        AppHost.AddRequests(host, builder.Services);
        AppHost.AddValidation(builder.Services);
        AppHost.AddClassNames(host, builder.Services);
        AppHost.AddRealtime(host, builder.Services);
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(cookie => cookie.LoginPath = SessionNotesEndpoints.SignIn);
        if (builder.Environment.IsDevelopment()) builder.Services.AddWorld();
        builder.Services.AddScoped<MoveInReads>();
        AppHost.AddTracing(host, builder);

        var app = builder.Build();
        app.UseRequestCultures();
        app.UseRequestZones();
        if (app.Environment.IsDevelopment())
        {
            app.UseWorld();
            app.UseWorldLines();
            app.MapWorld();
            app.MapAdmissionSession();
        }

        app.UseStaticFiles();
        if (host == HostNames.PublicInformation)
        {
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path;
                if (HttpMethods.IsGet(context.Request.Method) && (path == CarePages.AdmissionInformation || path == CareEndpoints.AdmissionDeskStatus || path == WorldEndpoints.Ready)
                    || app.Environment.IsDevelopment() && HttpMethods.IsPost(context.Request.Method) && path == WorldEndpoints.End)
                {
                    await next(context);
                    return;
                }
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("This page is unavailable on the public admissions desk.");
            });
        }
        AppHost.UseTracing(host, app);
        app.MapControllers();
        app.MapLiveLines();
        app.MapHub<MoveInCoordinatorHub>(CareEndpoints.MoveInCoordinatorLine);
        app.MapBenchLine();
        if (app.Configuration[TestHost] is { } testHost) StopWith(testHost, app.Lifetime);
        app.Run();
    }

    private static void StopWith(string testHost, IHostApplicationLifetime lifetime)
        => _ = Process.GetProcessById(int.Parse(testHost, CultureInfo.InvariantCulture)).WaitForExitAsync().ContinueWith(_ => lifetime.StopApplication(), TaskScheduler.Default);
}
