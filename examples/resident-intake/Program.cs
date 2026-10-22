using Alis.Reactive.FluentValidator;
using Alis.Reactive.Fusion;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddReactiveFluentValidation(validation =>
    validation.AddFromAssemblyContaining<Program>());
builder.Services.AddReactiveFusion();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Intake}/{action=Index}/{id?}");

app.Run();
