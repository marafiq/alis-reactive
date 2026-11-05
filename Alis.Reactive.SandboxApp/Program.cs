using Alis.Reactive;
using Alis.Reactive.Fusion;
using Alis.Reactive.SandboxApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

var sfLicense = builder.Configuration["Syncfusion:LicenseKey"];
if (!string.IsNullOrEmpty(sfLicense))
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(sfLicense);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => options.IdleTimeout = TimeSpan.FromHours(2));
builder.Services.AddReactiveFluentValidation(validation =>
    validation.AddFromAssemblyContaining<Program>());
builder.Services.AddReactiveFusion();
builder.Services.AddReactiveFluentValidation(validation =>
    validation.Add<Alis.Reactive.SandboxApp.Areas.Sandbox.Models.ReservationsOfficeValidator>());
builder.Services.AddSingleton(new Alis.Reactive.SandboxApp.Areas.Sandbox.Models.RateSheetPolicy("Maple Grove", 15000m));
builder.Services.AddSingleton<Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Brought.VisitBoardStreams>();

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ALIS_NO_BROADCAST")))
    builder.Services.AddHostedService<RealTimeBroadcastService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

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

public partial class Program;
