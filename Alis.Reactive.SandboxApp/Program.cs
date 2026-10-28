using Alis.Reactive.FluentValidator;
using Alis.Reactive.SandboxApp.Hubs;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// Syncfusion EJ2 DataSource rendering reads Newtonsoft defaults; keep sandbox API payloads camelCase.
JsonConvert.DefaultSettings = () => new JsonSerializerSettings
{
    ContractResolver = new CamelCasePropertyNamesContractResolver()
};

var builder = WebApplication.CreateBuilder(args);

// Optional Syncfusion license comes from configuration or user secrets.
var sfLicense = builder.Configuration["Syncfusion:LicenseKey"];
if (!string.IsNullOrEmpty(sfLicense))
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(sfLicense);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
// Per-session state keeps sandbox grid edits isolated across separate app/test sessions.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => options.IdleTimeout = TimeSpan.FromHours(2));
builder.Services.AddReactiveFluentValidation(validation =>
    validation.AddFromAssemblyContaining<Program>());
// The reservations office registers its own validators, as a module from its own assembly would; the
// registrations merge with the app's, and its hold-book rule runs on the server alone.
builder.Services.AddReactiveFluentValidation(validation =>
    validation.Add<Alis.Reactive.SandboxApp.Areas.Sandbox.Models.ReservationsOfficeValidator>());
// Maple Grove's rate policy: the rate sheet's validator takes it from DI, for the page and the server alike.
builder.Services.AddSingleton(new Alis.Reactive.SandboxApp.Areas.Sandbox.Models.RateSheetPolicy("Maple Grove", 15000m));
// The visit board's streams, keyed by session so browser worlds never hear each other.
builder.Services.AddSingleton<Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Brought.VisitBoardStreams>();

// Disable demo broadcasts in Playwright so realtime tests control their own events.
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ALIS_NO_BROADCAST")))
    builder.Services.AddHostedService<RealTimeBroadcastService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Sandbox consumes the built framework bundles directly from Assets/dist.
// Using the same path in dev and CI catches stale or missing bundles early.
var assetDistDir = Path.GetFullPath(
    Path.Combine(app.Environment.ContentRootPath, "..", "Alis.Reactive.Assets", "dist"));
if (!Directory.Exists(assetDistDir))
{
    throw new InvalidOperationException(
        $"Asset bundles not found at '{assetDistDir}'. " +
        "Run 'npm run build:all' from the repo root before starting the sandbox.");
}
app.Environment.WebRootFileProvider = new CompositeFileProvider(
    new PhysicalFileProvider(assetDistDir),
    app.Environment.WebRootFileProvider);

// Playwright boot must not depend on CDN availability. Syncfusion's ASP.NET
// helpers require global `ej/ejs` before the Reactive runtime boots.
var syncfusionPackageDir = Path.GetFullPath(
    Path.Combine(app.Environment.ContentRootPath, "..", "node_modules", "@syncfusion", "ej2"));
if (!Directory.Exists(syncfusionPackageDir))
{
    throw new InvalidOperationException(
        $"Syncfusion browser assets not found at '{syncfusionPackageDir}'. " +
        "Run 'npm install' from the repo root before starting the sandbox.");
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(syncfusionPackageDir),
    RequestPath = "/vendor/syncfusion"
});

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// A broken outage drill refuses the resident hub's negotiate, as an unreachable server would, so the
// page's automatic reconnect runs its schedule instead of being accepted and then closed by the hub.
app.Use(async (context, next) =>
{
    if (ResidentStatusHub.RefusesNegotiate(context.Request))
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        return;
    }

    await next(context);
});

app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<ResidentStatusHub>("/hubs/resident-status");

app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// The test projects start the sandbox by its entry point (AppServer).
public partial class Program;
