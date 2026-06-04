using Alis.Reactive;
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

app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<ResidentStatusHub>("/hubs/resident-status");

app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
