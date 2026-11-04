using Alis.Reactive;
using Alis.Reactive.Framework.App.Realtime;
using Alis.Reactive.Framework.App.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddRazorOptions(razor =>
{
    razor.ViewLocationFormats.Clear();
    razor.ViewLocationFormats.Add("/{1}/{0}.cshtml");
    razor.ViewLocationFormats.Add("/Shared/{0}.cshtml");
});
builder.Services.AddReactiveFluentValidation(validation => validation
    .AddFromAssemblyContaining<Program>()
    .Add<GuestNameRules<RegisteredGuest>>()
    .Add<NoteRules<HandoverNote>>()
    .Add<NoteRules<IncidentNote>>());
builder.Services.AddSignalR();
builder.Services.AddSingleton<LineExchange>();

var app = builder.Build();

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

public partial class Program;
