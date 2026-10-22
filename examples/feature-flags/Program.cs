using Alis.Reactive.FluentValidator;
using FeatureFlags.Domain;
using FeatureFlags.Models;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

var syncfusionLicense = builder.Configuration["Syncfusion:LicenseKey"];
if (!string.IsNullOrEmpty(syncfusionLicense))
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicense);

builder.Services.AddControllersWithViews();
builder.Services.AddReactiveFluentValidation(validation =>
    validation.AddFromAssemblyContaining<Program>());

// Fake data lives in memory for the life of the process.
builder.Services.AddSingleton(_ => Hierarchy.Seed());
builder.Services.AddSingleton<FlagRule>();
builder.Services.AddSingleton<ChangeText>();
builder.Services.AddSingleton<FlagStore>();
builder.Services.AddSingleton<FlagScreens>();

var app = builder.Build();

// Like the sandbox, this example serves the framework bundles straight from the repo
// (Alis.Reactive.Assets/dist and node_modules/@syncfusion/ej2), so it always runs the
// current source. A packaged app gets them from the NuGet targets instead.
var repoRoot = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", ".."));
var assetDist = RequireDirectory(Path.Combine(repoRoot, "Alis.Reactive.Assets", "dist"),
    "Run 'npm run build:all' from the repo root first.");
var syncfusionDist = RequireDirectory(Path.Combine(repoRoot, "node_modules", "@syncfusion", "ej2"),
    "Run 'npm install' from the repo root first.");

app.Environment.WebRootFileProvider = new CompositeFileProvider(
    new PhysicalFileProvider(assetDist),
    app.Environment.WebRootFileProvider);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(syncfusionDist),
    RequestPath = "/vendor/syncfusion"
});
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();

static string RequireDirectory(string path, string fix) =>
    Directory.Exists(path)
        ? path
        : throw new InvalidOperationException($"Browser assets not found at '{path}'. {fix}");
