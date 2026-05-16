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

ReactivePlanConfig.UseValidationExtractor(
    new FluentValidationAdapter(type => (FluentValidation.IValidator?)Activator.CreateInstance(type)));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Sandbox eats its own dog food as an Alis.Reactive consumer: serve the
// framework bundles straight from the sibling project dist/ trees produced by
// `npm run build:all`. Same path in dev and CI — no copy into sandbox wwwroot
// required, so `git status` stays clean after a local build.
//
//   Alis.Reactive.Assets/dist/        — runtime JS:    alis-reactive.dev.js
//   Alis.Reactive.DesignSystem/dist/  — design system: design-system.dev.css
//   Alis.Reactive.Fusion/dist/        — Fusion:        syncfusion.dev.css
//
// Fail fast if any dist folder is missing (Rule 5). The sandbox is a dev-only
// harness — there is no environment where it should boot without bundles.
// Silently skipping would leave it serving 404 on a bundle with no log signal,
// a trap for first-run devs who haven't run the JS/CSS build yet.
var assetDistDirs = new[]
{
    Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "Alis.Reactive.Assets", "dist")),
    Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "Alis.Reactive.DesignSystem", "dist")),
    Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "Alis.Reactive.Fusion", "dist")),
};
var assetProviders = new List<IFileProvider>();
foreach (var assetDistDir in assetDistDirs)
{
    if (!Directory.Exists(assetDistDir))
    {
        throw new InvalidOperationException(
            $"Asset bundles not found at '{assetDistDir}'. " +
            "Run 'npm run build:all' from the repo root before starting the sandbox.");
    }
    assetProviders.Add(new PhysicalFileProvider(assetDistDir));
}
assetProviders.Add(app.Environment.WebRootFileProvider);
app.Environment.WebRootFileProvider = new CompositeFileProvider(assetProviders);

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
