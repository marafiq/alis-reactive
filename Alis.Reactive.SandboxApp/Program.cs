using Alis.Reactive;
using Alis.Reactive.FluentValidator;
using Alis.Reactive.SandboxApp.Hubs;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// Global: Newtonsoft serializes PascalCase C# → camelCase JSON.
// SF EJ2 uses Newtonsoft internally for DataSource rendering —
// JsonConvert.DefaultSettings merges into SF's explicit settings.
JsonConvert.DefaultSettings = () => new JsonSerializerSettings
{
    ContractResolver = new CamelCasePropertyNamesContractResolver()
};

var builder = WebApplication.CreateBuilder(args);

// Syncfusion license — stored in user secrets: dotnet user-secrets set "Syncfusion:LicenseKey" "YOUR_KEY"
var sfLicense = builder.Configuration["Syncfusion:LicenseKey"];
if (!string.IsNullOrEmpty(sfLicense))
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(sfLicense);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Broadcast service pushes updates every 2s for demo — disabled during Playwright tests
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ALIS_NO_BROADCAST")))
    builder.Services.AddHostedService<RealTimeBroadcastService>();

ReactivePlanConfig.UseClientValidationRuleSource(
    new FluentValidationAdapter(type => (FluentValidation.IValidator?)Activator.CreateInstance(type)));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Sandbox eats its own dog food as an Alis.Reactive consumer: serve the
// framework bundles straight from Alis.Reactive.Assets/dist/, produced by
// `npm run build:all`. All three framework bundles build into that one tree:
//
//   Alis.Reactive.Assets/dist/scripts/alis-reactive.dev.js   — runtime JS
//   Alis.Reactive.Assets/dist/css/design-system.dev.css      — design system
//   Alis.Reactive.Assets/dist/css/syncfusion.dev.css         — Fusion
//
// Same path in dev and CI — no copy into sandbox wwwroot, so `git status`
// stays clean after a local build. Fail fast if the dist folder is missing
// (Rule 5): the sandbox is a dev-only harness, and silently serving 404 on a
// bundle is a trap for first-run devs who haven't run the build yet.
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
// helpers emit scripts that require the global `ej/ejs` object before the
// reactive runtime boots, so serve the npm package from the local workspace.
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
app.UseAuthorization();

app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<ResidentStatusHub>("/hubs/resident-status");

app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
