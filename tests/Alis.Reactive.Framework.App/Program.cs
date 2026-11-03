using Alis.Reactive;
using Alis.Reactive.Framework.App.Realtime;
using Alis.Reactive.Framework.App.Validation;

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

// The components' bundle built from the current source; a page without it would prove nothing, so the app
// refuses to start. The runtime bundle and the core's stylesheet come with the AlisReactive project reference
// the way an app gets them from the package, at ~/_content/AlisReactive/.
RequireBuilt(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "js", "framework-components.js"));

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
        throw new InvalidOperationException($"'{bundle}' is not built. Build the app: dotnet build tests/Alis.Reactive.Framework.App.");
}

// The framework's Playwright project starts the app by its entry point (AppServer).
public partial class Program;
