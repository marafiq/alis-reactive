using Alis.Reactive.FluentValidator;
using Alis.Reactive.Framework.App.Realtime;
using Alis.Reactive.Framework.App.Validation;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// A slice is one folder: its controller, its pages and their models. A controller named for its
// folder finds its views there.
builder.Services.AddControllersWithViews().AddRazorOptions(razor =>
{
    razor.ViewLocationFormats.Clear();
    razor.ViewLocationFormats.Add("/{1}/{0}.cshtml");
    razor.ViewLocationFormats.Add("/Shared/{0}.cshtml");
});
// Scanning finds every validator but a generic one, which the app registers for each model it checks.
builder.Services.AddReactiveFluentValidation(validation => validation
    .AddFromAssemblyContaining<Program>()
    .Add<GuestNameRules<RegisteredGuest>>()
    .Add<NoteRules<HandoverNote>>()
    .Add<NoteRules<IncidentNote>>());
builder.Services.AddSignalR();
builder.Services.AddSingleton<LineExchange>();

var app = builder.Build();

// The runtime bundle built from the current source, at the URL an app serves it from, and the
// components' bundle; a page without either would prove nothing, so the app refuses to start.
var root = app.Environment.ContentRootPath;
var runtimeDir = Path.GetFullPath(Path.Combine(root, "..", "..", "Alis.Reactive.Assets", "dist", "scripts"));
RequireBuilt(Path.Combine(runtimeDir, "alis-reactive.dev.js"));
RequireBuilt(Path.Combine(root, "wwwroot", "js", "framework-components.js"));

app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(runtimeDir), RequestPath = "/scripts" });
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.MapHub<Alis.Reactive.Framework.App.Realtime.RoundHub>("/hubs/round");
app.MapLineHub("/hubs/line");
app.MapControllerRoute("slices", "{controller=Home}/{action=Index}");
app.Run();

static void RequireBuilt(string bundle)
{
    if (!File.Exists(bundle))
        throw new InvalidOperationException($"'{bundle}' is not built. Run 'npm run build:all' from the repository root.");
}

// The framework's Playwright project starts the app by its entry point (AppServer).
public partial class Program;
