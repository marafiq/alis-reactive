using Alis.Reactive.FluentValidator;
using Alis.Reactive.Fusion;
using Newtonsoft.Json;

// Syncfusion renders data sources and its own options with Newtonsoft defaults: camel case
// the example's data, keep the names Syncfusion pins (Toast and Dialog position "X", "Y").
JsonConvert.DefaultSettings = () => new JsonSerializerSettings
{
    ContractResolver = FusionJson.ContractResolver
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddReactiveFluentValidation(validation =>
    validation.AddFromAssemblyContaining<Program>());

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Intake}/{action=Index}/{id?}");

app.Run();
